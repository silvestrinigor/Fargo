# Web API Getting Started

This guide shows the typical first calls against the Fargo Web API. Examples use
`curl`; replace the base address and credentials as needed.

## 1. Authenticate

Exchange credentials for an access token and a refresh token:

```bash
curl -X POST https://localhost:7563/identity/login \
  -H "Content-Type: application/json" \
  -d '{ "nameid": "admin", "password": "<password>" }'
```

Response:

```json
{
  "accessToken": "<jwt>",
  "refreshToken": "<token>",
  "expiresAt": "2026-10-20T16:33:15Z"
}
```

## 2. Call an authenticated endpoint

Send the access token as a bearer token:

```bash
curl https://localhost:7563/articles \
  -H "Authorization: Bearer <access-token>"
```

Add paging and partition filters as query parameters:

```bash
curl "https://localhost:7563/articles?page=1&limit=20&childOfAnyOfThesePartitions=<partition-guid>" \
  -H "Authorization: Bearer <access-token>"
```

## 3. Create a resource

```bash
curl -X POST https://localhost:7563/articles \
  -H "Authorization: Bearer <access-token>" \
  -H "Content-Type: application/json" \
  -d '{ "name": "Widget", "articleType": 1 }'
```

The response is the identifier of the created article.

## 4. Refresh the token when it expires

```bash
curl -X POST https://localhost:7563/identity/refresh \
  -H "Content-Type: application/json" \
  -d '{ "refreshToken": "<refresh-token>" }'
```

## 5. Inspect the audit trail

Every create, update, and delete is recorded. Fetch a single audit log,
including its metadata, by GUID:

```bash
curl https://localhost:7563/audit-logs/<audit-log-guid> \
  -H "Authorization: Bearer <access-token>"
```

## Using the CLI instead

The `fargo` command-line client wraps these calls, stores your session token,
and targets a configurable server:

```bash
fargo config set-server --url https://localhost:7563
fargo identity login --username admin
fargo articles list
fargo audit get <audit-log-guid>
```

## Next steps

See the [API Reference](reference/fargo.swagger.json) for the complete list of endpoints, request
parameters, and response schemas.
