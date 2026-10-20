# Web API Overview

The Fargo Web API is a REST API over HTTP that returns JSON. This page explains
the conventions that apply across all endpoints. For the full, generated list of
endpoints, request parameters, and response schemas, see the
[API Reference](reference/fargo.swagger.json).

## Base address

All endpoints are served relative to the HTTP API's base address (for example
`https://localhost:7563` in local development).

## Resources

The API is organized around these resource groups:

| Group | Base path | Description |
| --- | --- | --- |
| Identity | `/identity` | Authentication: login, logout, token refresh, password change. |
| Articles | `/articles` | Product catalog definitions. |
| Items | `/items` | Physical instances of articles and their locations. |
| Partitions | `/partitions` | The hierarchy used to scope and group data. |
| Users | `/users` | User accounts. |
| User groups | `/user-groups` | Collections of users that share permissions. |
| Audit logs | `/audit-logs` | Immutable record of changes. |

## Authentication

Most endpoints require a bearer token. Obtain one by posting credentials to
`POST /identity/login`, then send it on subsequent requests:

```http
Authorization: Bearer <access-token>
```

Tokens expire; use `POST /identity/refresh` with your refresh token to obtain a
new access token without logging in again.

A request without a valid token receives `401 Unauthorized`. A request from an
authenticated actor that lacks permission for the action receives
`403 Forbidden`.

## Pagination

List endpoints accept `page` and `limit` query parameters:

- `page` — 1-based page number.
- `limit` — maximum number of items per page.

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
  "instance": "/partitions",
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
