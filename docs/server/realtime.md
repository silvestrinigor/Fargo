# Real-time updates (SignalR)

Clients such as live dashboards can receive a push when an entity changes,
instead of polling the list endpoints. The push carries no entity data: the
client reads the entity again through the REST API, which applies the actor's
permissions and partition access to that read.

Today only **articles** produce events (`created`, `edited`, `deleted`).

## Architecture

```
command commits ──► outbox row (same transaction)
                      │
                      ▼
          Fargo.ServiceOutbox ──► RabbitMQ topic exchange "fargo.events"
                                          │  routing key article.*
                                          ▼
                              Fargo.ServiceRealtime (one queue per instance)
                                          │  partition check per connection
                                          ▼
                                   SignalR clients
```

- **Only committed changes are pushed.** The outbox row is written by the
  save-changes interceptor in the same transaction as the change, so a change
  that rolls back produces no event.
- **A separate host.** `Fargo.ServiceRealtime` holds the long-lived WebSocket
  connections, so the REST API (`Fargo.ServiceHttp`) stays stateless and has no
  broker dependency. Restarting or scaling the API does not drop connections, and
  if the real-time service is down, REST keeps working.
- **One queue per instance.** Each instance declares its own exclusive,
  auto-delete, server-named queue bound to `article.*`. Every instance receives
  every event and sends it to its own connections, so no SignalR backplane (such
  as Redis) is needed to run several instances.

## The event message

Article events are published with schema version **2**:

```json
{
  "eventId": "6f1c2b3a-0000-7000-8000-000000000001",
  "eventType": "article.edited",
  "occurredAt": "2026-11-06T14:03:22+00:00",
  "articleGuid": "a3e9d1c2-0000-4000-8000-000000000002",
  "partitionGuids": ["10000000-0000-4000-8000-000000000001"]
}
```

`partitionGuids` holds every partition the article belonged to **before or
after** the change. When a partition is removed, the actors that just lost
access are told too, and their next read returns not found. A deleted article
reports the partitions it had when it was deleted. The `schema-version` message
header carries the version, and the consumer drops any other version instead of
guessing.

## Who receives an event

When a client connects, the hub resolves the actor with `ActorResolver`. That
gives the same partition access, expanded down the partition tree, that the API
uses. The hub keeps that access with the connection. An event goes to a
connection only when the actor can access **at least one** of the event's
partitions. This is the same rule the API applies to article reads.

An event with no partitions reaches nobody. This is deliberately stricter than
`SecurityPrincipal.HasAccess`, which allows entities without partitions, so a
connection never learns about something it could not read through the API.

Access is kept current in three ways:

| Trigger | Effect |
|---|---|
| Every `Realtime:AccessRefreshInterval` (default 1 minute) | The access of every connected actor is read again; a deleted or deactivated actor is disconnected. |
| Access token expires | The connection is closed (`CloseOnAuthenticationExpiration`). The client reconnects with a fresh token, which is validated again. |
| Connect | The token is validated like an API token, including the active-state and auth-version check, so a logged-out token is refused. |

## Connecting

- **Hub:** `/hubs/fargo` on the real-time service (the `fargo-realtime`
  resource in Aspire).
- **Authentication:** a Fargo access token, from a user or a service account.
  Send it in the `Authorization: Bearer` header, or, for browsers, which cannot set
  headers on a WebSocket handshake, in the `access_token` query parameter. The
  query parameter is accepted under `/hubs` only. Query strings can end up in
  proxy and access logs, so keep them out of those logs for hub paths.
- **Client method:** `EntityChanged`, with this payload:

```json
{
  "entityType": "article",
  "guid": "a3e9d1c2-0000-4000-8000-000000000002",
  "changeType": "edited",
  "eventId": "6f1c2b3a-0000-7000-8000-000000000001",
  "occurredAt": "2026-11-06T14:03:22+00:00"
}
```

`changeType` is `created`, `edited` or `deleted`.

A .NET client:

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://realtime.example/hubs/fargo", options =>
        options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken))
    .WithAutomaticReconnect()
    .Build();

connection.On<EntityChanged>("EntityChanged", change =>
{
    // Re-fetch change.Guid through the REST API.
});

connection.Reconnected += _ => RefreshEverythingAsync();

await connection.StartAsync();
```

### What clients must handle

- **Duplicates.** Delivery is at-least-once, so the same `eventId` can arrive
  twice. Re-fetching is idempotent, so most clients can ignore this.
- **Missed events.** Events are not stored for disconnected clients. After a
  reconnect, re-read the data that is on screen.
- **Latency.** About a second from commit to push: the outbox relay polls every
  `Outbox:PollInterval` (default 1 second) when idle.

## Configuration

| Section | Key | Default | Meaning |
|---|---|---|---|
| `Realtime` | `AccessRefreshInterval` | `00:01:00` | How often the access of connected actors is read again. |
| `RabbitMqEvents` | `ExchangeName` | `fargo.events` | The topic exchange, shared with the publisher. |
| `RabbitMqConsumer` | `PrefetchCount` | `50` | Unacknowledged messages delivered ahead. |
| `RabbitMqConsumer` | `RetryDelay` / `MaxRetryDelay` | `1s` / `30s` | Backoff when the broker is unreachable or the channel closes. |

The service also needs the `Jwt` settings the API uses, so it validates the same
tokens, plus the database (`fargo-database`) and broker (`fargo-rabbitmq`)
connection strings.

## Failure behaviour

- **Broker down.** The hub keeps accepting connections. The consumer retries
  with backoff and pushes resume when the broker is back. Events wait in the
  outbox in the meantime.
- **Outbox relay down.** No events reach the broker, so nothing is pushed until
  the relay is back.
- **Malformed message, or another schema version.** It is logged, acknowledged
  and dropped. It is never requeued, so it cannot block the queue.
- **Database down.** New connections cannot be authorized, and the periodic
  refresh keeps the last known access until the database is back.

## Code map

| Piece | Location |
|---|---|
| Message contract | `Fargo.Application/IntegrationEvents/ArticleEvents.cs`, `IntegrationEventJson.cs` |
| Handler abstraction | `Fargo.Application/IntegrationEvents/IArticleEventHandler.cs` |
| Partitions in the message | `Fargo.Infrastructure/Outbox/OutboxSaveChangesInterceptor.cs` |
| Broker consumer | `Fargo.Infrastructure/Messaging/RabbitMqArticleEventConsumer.cs` |
| Hub, routing, access refresh | `Fargo.ServiceRealtime` (`Hubs/`, `Notifications/`, `Connections/`, `Services/`) |

## Not done yet

- **A client helper package** (for example `Fargo.ClientRealtime`), with
  reconnect and typed events.
- **Events for other entities** (items, partitions, and so on). Each needs its
  own events in the outbox interceptor.
- **CORS.** Browser clients on another origin need it configured, or a reverse
  proxy that serves `/hubs/*` from the same origin as the app.
- **Pushing access changes.** Once events are published for changes to an
  actor's access, the service can refresh on those instead of on a timer.
