# Idempotency

Idempotency keys let a client retry a request without repeating its effect. This
page explains how the server does it, where the keys are stored, and how they
are cleaned up. For how clients use the header, see
[Idempotency Keys](../web-api/idempotency.md).

## What is protected

A command opts in by implementing `IIdempotentCommand`. Three commands do, the
ones with no natural guard against a retry:

| Command | Endpoint |
| --- | --- |
| `SupplyOrderReceiveCommand` | `POST /v1/supply-orders/{guid}/receive` |
| `ItemEventCreateCommand` | `POST /v1/item/events` |
| `ItemCreateCommand` | `POST /v1/items` |

Adding a command to the list is a deliberate decision, and a test lists the
current set. Never opt in a command whose response holds a secret, because the
response is stored to be replayed. Creating a service account and rotating its
secret are excluded for that reason.

## How a request is handled

The idempotency decorator sits between the unit of work and the audit decorator:

`Logging(unit of work(Idempotency(Audit(handler))))`

For a command with a key, it looks up a record by caller and key:

1. **No record, or an expired one:** the command runs. The decorator adds a record
   with the response, and the unit of work saves it together with the command's
   changes.
2. **A record for the same command:** the stored response is returned. The
   handler does not run, so nothing is audited or published a second time.
3. **A record for another command:** the request is rejected with `422`, because
   the stored response would not fit.

Because the record is saved in the same transaction as the change, it exists
only if the change committed. A failed request leaves nothing behind and can be
retried. Two requests with the same key at the same moment both run, but the
unique key lets only one save succeed. The other changes roll back with its
failed save, and it gets a `409` with `Retry-After`; its retry returns the
winner's result.

The key identifies the operation, not the payload. The server does not compare
the request body on a replay. Replay does not check permissions again: it only
returns a result that the same caller was already allowed to produce.

## Storage

Records live in the `idempotency_keys` table, keyed by the caller's GUID and the
key. Each row keeps the command name, the response (a GUID or nothing), and its
creation and expiry times. Deleting an actor deletes its records.

Each record stores its own expiry, written when it is created from the API's
`Idempotency:Retention` setting. A record that has expired is treated as absent
and is reused by the next request with that key. Two requests racing to reuse it
cannot both succeed, because the row carries the `xmin` concurrency token.

## Cleanup

`Fargo.ServiceMaintenance` deletes expired records. It runs once at startup and
then every `PurgeInterval`, deleting in batches so that no statement holds locks
for long. A failed run is logged and tried again at the next interval, because
expired records only waste space. Several instances can run at once, since
deleting a record twice is harmless.

The service needs only the database. The outbox relay keeps purging its own
published messages; that is a separate concern.

## Configuration

| Setting | Service | Default | Meaning |
| --- | --- | --- | --- |
| `Idempotency:Retention` | API | `1.00:00:00` | How long a key protects a request. Must be greater than zero. |
| `Maintenance:IdempotencyKeys:PurgeInterval` | Maintenance | `01:00:00` | How often expired records are deleted. |
| `Maintenance:IdempotencyKeys:BatchSize` | Maintenance | `1000` | Records deleted per statement. |

The purge does not read the retention setting, so the two services cannot
disagree about when a key ends.