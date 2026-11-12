# Web API Overview

The Fargo Web API is a REST API over HTTP that returns JSON. This page explains
the conventions that apply across all endpoints. For the full, generated list of
endpoints, request parameters, and response schemas, see the
[API Reference](reference/fargo.swagger.json).

## Base address

All endpoints are served relative to the HTTP API's base address (for example
`https://localhost:7563` in local development).

## Versioning

Every endpoint is versioned through a required `v{version}` segment at the start
of its path — for example `GET /v1/articles`. The current version is **v1**.

Versioning is **per endpoint**: an endpoint advertises only the versions it
actually implements. When a single endpoint gets a breaking change it gains a
new version (for example `v2`) while every other endpoint stays on `v1`, so a
request to a version an endpoint does not implement returns `404 Not Found`.
This lets clients migrate one call at a time instead of all at once.

Each version has its own OpenAPI document, served at `/openapi/v{version}.json`
(for example `/openapi/v1.json`). Responses also carry an
`api-supported-versions` header listing the versions the endpoint supports.

## Resources

The API is organized around these resource groups (shown at the current `v1`):

| Group | Base path | Description |
| --- | --- | --- |
| Identity | `/v1/identity` | Authentication: login, logout, token refresh, password change. |
| Articles | `/v1/articles` | Product catalog definitions. |
| Items | `/v1/items` | Physical instances of articles and their locations. |
| Item events | `/v1/item/events` | Stock changes (movement, creation, deletion) for event-tracked articles. |
| Partitions | `/v1/partitions` | The hierarchy used to scope and group data. |
| Users | `/v1/users` | User accounts. |
| User groups | `/v1/user-groups` | Collections of users that share permissions. |
| Audit logs | `/v1/audit-logs` | Immutable record of changes. |

## Authentication

Most endpoints require a bearer token. Obtain one by posting credentials to
`POST /v1/identity/login`, then send it on subsequent requests:

```http
Authorization: Bearer <access-token>
```

Tokens expire; use `POST /v1/identity/refresh` with your refresh token to obtain
a new access token without logging in again.

A request without a valid token receives `401 Unauthorized`. A request from an
authenticated actor that lacks permission for the action receives
`403 Forbidden`.

## Pagination

List endpoints accept `page` and `limit` query parameters:

- `page` — 1-based page number.
- `limit` — maximum number of items per page.

## Sorting and search

List endpoints accept two optional query parameters:

- `sort` — the field to order by. Prefix with `-` for descending order
  (for example `sort=name` ascending, `sort=-name` descending). Each resource
  accepts a fixed set of sort fields; requesting an unknown field returns
  `400 Bad Request` with the list of allowed fields. Results always have a
  stable secondary order by GUID, so paging is deterministic. When `sort` is
  omitted, each resource keeps its natural default order.
- `search` — case-insensitive text matched as a substring against the resource's
  natural text field(s): article/partition name and description, user and user
  group nameid, and the article barcode for items. The total count reflects the
  search filter.

Example: `GET /v1/articles?search=bolt&sort=-name&page=1&limit=20`.

## Partition filtering

List endpoints that return partitioned data accept
`childOfAnyOfThesePartitions` — one or more partition GUIDs that scope the
results to those partitions and their descendants.

## Errors

Errors are returned as [RFC 7807 problem details](https://datatracker.ietf.org/doc/html/rfc7807)
with `Content-Type: application/problem+json`. A typical body:

```json
{
  "status": 403,
  "title": "Permission denied.",
  "detail": "Action 'CreatePartition' permission denied for actor '...' of type 'User'.",
  "instance": "/v1/partitions",
  "traceId": "00-...-01",
  "errorType": "..."
}
```

Common status codes:

| Status | Meaning |
| --- | --- |
| `200 OK` | The request succeeded and a body is returned. |
| `204 No Content` | The request succeeded with no body (e.g. an empty list or a successful update/delete). |
| `400 Bad Request` | The request was malformed or failed validation. |
| `401 Unauthorized` | Authentication is required, missing, or expired. |
| `403 Forbidden` | The authenticated actor lacks permission. |
| `404 Not Found` | The requested resource does not exist or is not accessible. |
| `429 Too Many Requests` | A rate limit was exceeded. |

## Rate limiting

Authentication endpoints and general authenticated traffic are rate limited.
Exceeding a limit returns `429 Too Many Requests`.

## Next steps

- [Getting Started](getting-started.md) — authenticate and make your first calls.
- [API Reference](reference/fargo.swagger.json) — the complete, generated endpoint reference.
