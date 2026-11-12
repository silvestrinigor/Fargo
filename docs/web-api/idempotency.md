# Idempotency Keys

A network failure can leave a client not knowing whether a request worked. For
most requests the answer is simple: retry it. For a few, a retry would do the
work twice. An **idempotency key** makes those requests safe to retry: send the
same key again and the API returns the first result instead of repeating the
operation.

## Endpoints that accept a key

Only the endpoints where a repeat would do damage take a key. The others either
set a value, so repeating them changes nothing, or are rejected on the second
attempt.

| Endpoint | What a repeat would do without a key |
| --- | --- |
| `POST /v1/supply-orders/{guid}/receive` | Receive the goods again. Over-receipt is allowed, so the quantity is counted twice. |
| `POST /v1/item/events` | Record the stock event again. |
| `POST /v1/items` | Create a second item. |

## How it works

Send a GUID in the `Idempotency-Key` header:

```http
POST /v1/supply-orders/3f2b8c1e-9d4a-4b7e-8c2f-1a5d6e7f8091/receive
Authorization: Bearer <access-token>
Idempotency-Key: 7c1d4f52-6a3e-4b0e-9f2a-2d5b8e1c3a47
Content-Type: application/json
```

- The first request with a key runs normally and its result is kept.
- A later request with the same key returns that result and does nothing else.
  The response has an `Idempotency-Replayed: true` header.
- The key is optional. Without it the request runs every time.
- A key belongs to the caller that used it. Two callers can use the same key
  without affecting each other.
- A request that fails is not remembered, so you can retry it with the same key.

## Choosing a key

Generate a new GUID for each operation you want to do once, and reuse it only
to retry that operation.

The key identifies the operation, not the request body. If you change the
request, use a new key: the same key with a different body returns the first
result and ignores the new body. A script that may run twice can derive its key
from the work it is doing, for example a name-based UUID (version 5) built from
the order and the delivery it is receiving.

## Responses

| Status | Meaning |
| --- | --- |
| `200 OK` or `204 No Content` | The normal result, whether the request ran now or is replayed. |
| `400 Bad Request` | The key is not a GUID, or is the empty GUID (all zeros). |
| `409 Conflict` | Another request with the same key is running or has just finished. The response has `Retry-After: 1`. Retry to receive its result. |
| `422 Unprocessable Content` | The key was already used for a different endpoint. |

## How long a key lasts

A key is remembered for 24 hours by default. After that the same key counts as
new. The server can change this period; see
[Idempotency](../server/idempotency.md).

A replayed request still counts toward the [rate limit](overview.md#rate-limiting).

## Clients

The Fargo CLI sends a new key on every command that creates stock, which
protects its own automatic retries. Pass `--idempotency-key` to reuse a key
across runs:

```bash
fargo item event create --article <article-guid> --quantity 10 --container <container-guid> --idempotency-key <guid>
```

The MCP tools that create items and item events take an optional
`idempotencyKey` and generate one when it is omitted. In code, the
`Fargo.ClientHttp` library offers `IdempotencyKeys.Header(key)`, which you pass
as the request configuration of the generated client.

## Next steps

- [Web API Overview](overview.md) — conventions that apply to every endpoint.
- [API Reference](reference/fargo.swagger.json) — the complete endpoint reference.