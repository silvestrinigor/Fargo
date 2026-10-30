# Real-time updates (SignalR) — implementation plan

> Deferred from **GAP-16**. The sorting and general-search parts of GAP-16 are
> implemented; this document captures the remaining **real-time push** piece as a
> separate, larger feature to implement later. Tier 3.

## Goal

Let clients — chiefly live dashboards — receive updates when inventory/entities
change, instead of polling the list endpoints. A dashboard showing stock levels
or recent movements should update within a second of a change, without a refresh.

## Why it is separate from sorting/search

Sorting and search are read-path query features that slot into the existing
`endpoint → query → handler → repository` pipeline with no architectural change.
Real-time push is a different kind of work: a persistent transport, connection
lifecycle management, authentication on the socket, and a change-notification
path out of the command pipeline. It has a different risk and review surface, so
it is built on its own rather than bundled with the query features.

## Design

### Transport
- Add the `Microsoft.AspNetCore.SignalR` server package and a `FargoHub : Hub`
  under `src/server/Fargo.ServiceHttp/RealTime/`. Map it at `/hubs/fargo`.
- Require authentication on the hub (`[Authorize]`) and reuse the existing JWT
  bearer scheme. Browsers cannot set `Authorization` headers on the WebSocket
  handshake, so configure `JwtBearerOptions.Events.OnMessageReceived` to read the
  token from the `access_token` query-string parameter **for hub paths only**.
  Service-account tokens work the same way (the actor-type claim is already on
  the token), so a service could also subscribe.

### What is pushed
- Start coarse and safe: an `EntityChanged { EntityType, Guid, ChangeType }`
  message (ChangeType = Created / Updated / Deleted). **No entity payload
  initially** — the client reacts by re-fetching through the REST endpoint it
  already uses, so partition and permission scoping are re-enforced on that read.
  This avoids duplicating authorization logic in the push path.

### Where events originate
- The command pipeline already has decorator seams
  (`LoggingCommandHandlerDecorator`, `UnitOfWork*/Transactional*` decorators in
  `Fargo.Application/Common`). Introduce an `INotificationCollector` that command
  handlers (or a dedicated decorator) populate with `EntityChanged` facts, and
  **publish only after the transaction commits** — never push an uncommitted
  change. A change that rolls back must produce no notification.
- This is the same shape as an outbox, so it should be designed together with
  **GAP-14 (outbound integration / eventing)**: build one post-commit
  notification mechanism and feed both SignalR and any future webhooks/bus.

### Partition & permission scoping (critical)
- Do **not** broadcast globally. On connect, resolve the actor (reusing
  `ActorResolver`) and add the connection to a SignalR group per partition it can
  access (`Groups.AddToGroupAsync`, honoring the parent→descendant expansion the
  actor already carries). Publish each `EntityChanged` only to the groups for the
  changed entity's partitions.
- Invariant: a connection must never learn about an entity it could not read via
  REST. When in doubt, send less — the client re-fetches and the REST layer is
  the authority.
- Handle revocation: drop or re-authorize a connection when the user/service is
  deactivated or its auth-version changes, mirroring the per-request token
  re-validation in `FargoJwtBearerOptionsExtensions`.

### Client
- Add a SignalR client helper (in `Fargo.ClientHttp` or a sibling package) that
  connects with `HubConnectionBuilder().WithAutomaticReconnect()` and exposes a
  typed stream of `EntityChanged`. Surface it only where a real consumer needs it
  (dashboards), **not** in the human CLI.

## Tasks
1. Hub + DI + endpoint mapping; JWT-on-handshake for hub paths.
2. `INotificationCollector` + post-commit publish from the command pipeline
   (shared with the GAP-14 outbox if built together).
3. Partition-scoped groups on connect; publish `EntityChanged` to the right
   groups.
4. Connection teardown / re-authorization on deactivation or auth-version change.
5. Client helper with auto-reconnect; an integration test with a real connection.
6. Load / backpressure review: fan-out per partition, max connections, event
   rate, and a multi-instance backplane decision (below).

## Decisions to make before starting
- **Event scope**: which aggregates emit events, and at what granularity
  (Created/Updated/Deleted vs finer domain events like "item moved").
- **Payload-less vs payload-bearing**: recommend payload-less (re-fetch) first;
  payload-bearing is faster but must re-apply partition/permission scoping in the
  push path.
- **Backplane**: a single API instance needs no backplane; multiple instances do
  (Redis is the usual choice for SignalR). The current VPN-internal single-instance
  deployment may not need one yet — decide based on how the API is actually hosted.
- **Build with GAP-14**: the post-commit notification/outbox should be designed
  once and serve both real-time push and outbound integrations.
