# Cross-Cutting Concerns

This page covers concerns that apply across the whole HTTP surface: error
handling and rate limiting, plus the defaults every host shares: telemetry,
health checks, and resilience.

## Error handling

Exceptions are translated into
[RFC 7807 problem details](https://datatracker.ietf.org/doc/html/rfc7807) by a
chain of exception handlers, registered with `AddFargoExceptionHandler` and
`AddProblemDetails`. `UseStatusCodePages` ensures bodyless error status codes
also return a consistent response.

| Exception | Status |
| --- | --- |
| `InvalidCredentials` | `401 Unauthorized` |
| `UserPasswordChangeRequired` | `401 Unauthorized` |
| `ActorAccessDenied` | `403 Forbidden` |
| `ActorPermissionDenied` | `403 Forbidden` |
| `ActorNotFound` | `403 Forbidden` |
| `EntityNotFound` | `404 Not Found` |
| A domain or application error of type `Conflict` | `409 Conflict` |
| A concurrency conflict (`DbUpdateConcurrencyException`): the data changed since it was read; read it again and retry | `409 Conflict` |
| A unique-constraint violation in the database, for example a name already in use | `409 Conflict` |
| `AuthenticationRateLimit` | `429 Too Many Requests` |
| An infrastructure failure (`FargoInfrastructureException`) | `503 Service Unavailable` |
| A malformed request (`BadHttpRequestException`), and other domain or application errors | `400 Bad Request` |

Every problem response includes a `title`, a `detail`, the request `instance`,
and extension members: a `traceId` for correlation, an `errorType`, and any
context identifiers relevant to the error (for example the actor, action, or
entity involved).

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

## Rate limiting

Rate limiting is configured by `AddFargoRateLimiter` from the `RateLimiting`
section and applied with `UseRateLimiter`. Rejected requests receive
`429 Too Many Requests`, with a `Retry-After` header when known. All limiters
are fixed-window.

| Scope | Setting | Default limit | Partitioned by |
| --- | --- | --- | --- |
| Global (every request) | `Global` | 300 / minute | client IP |
| Authenticated resource endpoints | `GeneralAuthenticated` | 100 / minute | user identity, or client IP |
| Login | `IdentityLogin` | 10 / minute | client IP |
| Token refresh | `IdentityRefresh` | 30 / minute | client IP |
| Other identity endpoints | `IdentityDefault` | 10 / minute | client IP |

Each setting has `PermitLimit`, `WindowSeconds` and `QueueLimit` (`0`
rejects at once instead of queueing). `RateLimiting:Enabled = false` turns rate
limiting off. The global limiter applies to all traffic; endpoints additionally
opt into a named policy. The stricter identity limits protect the
authentication surface from brute-force attempts. Behind a proxy, enable
[forwarded headers](operations.md#behind-a-proxy) so the limits see the real
client IP.

## Host defaults

`Fargo.ServiceDefaults` provides the shared defaults applied by
`AddFargoServiceDefaults`, used by the service hosts.

### Telemetry (OpenTelemetry)

- **Logging** with formatted messages and scopes.
- **Metrics** from ASP.NET Core, `HttpClient`, and the .NET runtime.
- **Tracing** from ASP.NET Core (health-check requests are excluded) and
  `HttpClient`.
- When the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable is set, telemetry
  is exported via OTLP.

### Health checks

Two endpoints are mapped in every environment, so orchestrators and load
balancers can probe them. They return only the overall status, no component
details:

- `/health` — ready: all registered health checks must pass. The HTTP API and
  the real-time service also register a database check
  (`AddDbContextCheck<FargoDbContext>`).
- `/alive` — live: only checks tagged `live` (a default `self` check) must pass.

### Resilience and service discovery

Outbound `HttpClient` calls get the standard resilience handler (retries,
timeouts, circuit breaker) and service discovery by default.

## Other pipeline concerns

- **Response compression** (`AddResponseCompression` / `UseResponseCompression`).
- **HTTPS redirection**, and **HSTS** outside development.
- **OpenAPI** document generation; the document is mapped at `/openapi` in
  development and is the source of the [Web API Reference](~/web-api/reference/fargo.swagger.json).
