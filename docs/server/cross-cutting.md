# Cross-Cutting Concerns

This page covers concerns that apply across the whole HTTP surface: error
handling, rate limiting, and the shared host defaults (telemetry, health checks,
and resilience).

## Error handling

Domain and application exceptions are translated into
[RFC 7807 problem details](https://datatracker.ietf.org/doc/html/rfc7807) by
`FargoApplicationExceptionHandler`, registered with `AddFargoExceptionHandler`
and `AddProblemDetails`. `UseStatusCodePages` ensures bodyless error status codes
also return a consistent response.

Each application exception maps to a status code:

| Exception | Status |
| --- | --- |
| `InvalidCredentials` | `401 Unauthorized` |
| `UserPasswordChangeRequired` | `401 Unauthorized` |
| `ActorAccessDenied` | `403 Forbidden` |
| `ActorPermissionDenied` | `403 Forbidden` |
| `ActorNotFound` | `403 Forbidden` |
| `EntityNotFound` | `404 Not Found` |
| `AuthenticationRateLimit` | `429 Too Many Requests` |
| other application errors | `400 Bad Request` |

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

Rate limiting is configured by `AddFargoRateLimiter` and applied with
`UseRateLimiter`. Rejected requests receive `429 Too Many Requests`. All limiters
are fixed-window.

| Scope | Policy | Limit | Partitioned by |
| --- | --- | --- | --- |
| Global (every request) | — | 100 / minute | client IP |
| Authenticated resource endpoints | `general-authenticated` | 100 / minute | user identity |
| Login | `identity-login` | 10 / minute | client IP |
| Token refresh | `identity-refresh` | 30 / minute | client IP |
| Other identity endpoints | `identity-default` | 10 / minute | client IP |

The global limiter applies to all traffic; endpoints additionally opt into a
named policy. The stricter login/identity limits protect the authentication
surface from brute-force attempts.

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

Two endpoints are exposed **in development only** (exposing health endpoints in
other environments has security implications):

- `/health` — ready: all registered health checks must pass. The HTTP API also
  registers a database check (`AddDbContextCheck<FargoDbContext>`).
- `/alive` — live: only checks tagged `live` (a default `self` check) must pass.

### Resilience and service discovery

Outbound `HttpClient` calls get the standard resilience handler (retries,
timeouts, circuit breaker) and service discovery by default.

## Other pipeline concerns

- **Response compression** (`AddResponseCompression` / `UseResponseCompression`).
- **HTTPS redirection**, and **HSTS** outside development.
- **OpenAPI** document generation; the document is mapped at `/openapi` in
  development and is the source of the [Web API Reference](~/docs/web-api/reference/fargo.swagger.json).
