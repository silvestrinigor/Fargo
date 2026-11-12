# Security

Fargo authenticates callers with JWT bearer tokens and authorizes them with a
combination of **permissions** and **partition access**, resolved into a
`SecurityPrincipal`. Every state change is recorded in an audit log.

## Authentication

The HTTP host uses JWT bearer authentication. Token settings are bound and
validated from the `Jwt` configuration section into `JwtOptions`.

The security implementations live in `Fargo.Infrastructure/Security`:

| Concern | Interface | Implementation |
| --- | --- | --- |
| Password hashing | `IPasswordHasher` | `IdentityPasswordHasher` |
| Access token generation | `ITokenGenerator` | `JwtTokenGenerator` |
| Refresh token generation | `IRefreshTokenGenerator` | `CryptoRefreshTokenGenerator` |
| Refresh token hashing | `ITokenHasher` | `Sha256TokenHasher` |

### Token flow

The identity endpoints (`/identity`) implement the standard flow:

1. **Login** — valid credentials return a short-lived **access token** and a
   **refresh token**. Refresh tokens are stored hashed, never in plain text.
2. **Refresh** — a valid refresh token exchanges for a new access token without
   re-sending credentials.
3. **Logout** — invalidates the refresh token.
4. **Change password** — updates the password; a user may be required to change
   their password before continuing.

See the [Web API Overview](~/web-api/overview.md#authentication) for the
HTTP-level details.

### Service accounts

Non-interactive callers (integrations, scheduled jobs, other services) authenticate as **service accounts** rather than users. A service account has a client id and a client secret; the secret is generated server-side, returned **exactly once** at creation (or when rotated), and only ever stored hashed.

A service exchanges its credentials for a short-lived access token at `POST /identity/service-token` (the client-credentials flow). Unlike user login, no refresh token is issued - a service simply requests a new token when the current one nears expiry. The issued token carries an actor-type claim marking it as a service, so it resolves to a service principal.

Service accounts are managed under `/service-accounts` (create, read, update, rotate secret, delete), each operation gated by a dedicated permission (`CreateServiceAccount`, `ReadServiceAccount`, `EditServiceAccount`, `DeleteServiceAccount`). Deactivating an account, or rotating its secret, invalidates all tokens previously issued to it.

Like a user, a service account is a **partitioned** entity: it carries both a **partition membership** (the partitions it belongs to, which define its scope and drive partition-filtered listings and audit-log partition association) and **partition access** (the partitions it is allowed to reach, expanded down the partition hierarchy). When creating or updating an account, the granting actor must itself have access to every partition it assigns as membership or access. Unlike a user, a service account has no mandatory global-partition membership — it is a scoped machine identity whose memberships are assigned explicitly.

## The security principal

Requests act on behalf of a `SecurityPrincipal` — the authenticated identity
together with its resolved **permissions** and **partition access**. The current
identity is provided by `ICurrentActor` (`CurrentUserActor`), which reads the
token's actor-type claim so a request resolves as either an `ActorType.User` or
an `ActorType.Service` principal, and `ActorResolver` computes the principal's
effective permissions and accessible partitions, aggregating what is granted
directly to the actor and what is inherited through the user's group hierarchy.

Application handlers resolve the principal and enforce authorization before
touching domain state.

### How the principal is read, and cached

`ActorResolver` (Core) asks the `IActorAuthorizationProvider` port. Infrastructure
decides whether the answer comes from the cache or the database:

1. **Every request, uncached:** one small query reads the actor's active flag,
   its auth version and the current **authorization stamp**. Token validation
   uses the same read, which is kept for the rest of the request, so it runs
   once. Deactivation, logout and password changes therefore take effect on the
   next request, as before.
2. **Permissions and partition accesses:** these are the recursive group and
   partition queries. They come from a cache keyed by actor and stamp, and are
   loaded from the database only on a miss.

The stamp is a single-row table (`authorization_stamps`). Its value is replaced
by a new random `Guid`, in the same transaction, whenever an actor, a partition
access, a group membership, a user group, a group partition access or a
partition is added, changed or deleted (`AuthorizationStampSaveChangesInterceptor`).
A new stamp means a new cache key, so nothing ever has to be removed from the
cache and every instance sees the change at once. Any such change invalidates
every actor's entry. That's simpler than working out who is affected, and cheap
because these changes are rare.

Changes made outside the change tracker (raw SQL, `ExecuteUpdate`,
`ExecuteDelete`) don't renew the stamp. Renew it explicitly in such code paths.

| `ActorAuthorizationCache` setting | Default | Meaning |
|---|---|---|
| `Provider` | `Hybrid` | `Hybrid` uses .NET `HybridCache`, in memory. Registering an `IDistributedCache`, for example Redis with `AddStackExchangeRedisCache`, makes it a shared second level with no code change. `None` disables caching. |
| `Expiration` | `00:10:00` | A safety limit for unused entries. Freshness comes from the stamp, not from expiry. |

To use a different cache technology, register another
`IActorAuthorizationCache` (Infrastructure). Core and Application don't
reference any cache.

## Permissions

Permissions are values of [`ActionType`](xref:Fargo.Core.Actors.ActionType) — a
create / delete / edit permission per resource plus a few specific actions such
as `ChangeAnotherUserPassword`, `ChangeUserGroupMembers`, `ReadAuditLogs`,
`ReadItemEvents`, and the service-account actions (`CreateServiceAccount`, `ReadServiceAccount`, `EditServiceAccount`, `DeleteServiceAccount`).

Permissions can be granted directly to a user or to a user group; a user's
effective permissions are the union of both (including permissions inherited
through nested groups). A handler enforces a permission with:

```csharp
actor.ThrowIfPermissionDenied(ActionType.CreatePartition);
```

which raises a domain exception (surfaced as `403 Forbidden`) when the actor
lacks the permission.

## Partition scoping

Permissions decide *what actions* an actor may perform; partitions decide *which
data* an actor may see and touch. Each actor carries the set of partition GUIDs
it can access (resolved from the user and its groups, honoring parent-to-child
[inheritance](domain-model.md#partitions-and-access-inheritance)).

Access to a partitioned resource is granted when the resource has no partitions,
or when any of its partitions is in the actor's accessible set:

```csharp
// Fargo.Core.Actors.SecurityPrincipal
public bool HasAccess(IPartitionedGuidsReadOnly partitioned)
{
    if (partitioned.PartitionGuids.Count == 0)
    {
        return true;
    }

    return partitioned.PartitionGuids.Any(partitionAccessGuids.Contains);
}
```

Query repositories apply the same partition filter at the database level, so
listings and lookups never return entities outside the actor's scope.

## Audit logging

Every create, update, and delete produces an [audit log](domain-model.md#audit-logs)
recording the actor, the affected entity and its type, the
[`AuditLogType`](xref:Fargo.Core.Audits.AuditLogType), the timestamp, and a metadata
bag. Audit logs inherit the partition scope of the entity they describe, so they
are visible only to actors who can access that entity.

## Error responses

Authentication and authorization failures are returned as problem details:

- `401 Unauthorized` — invalid credentials, or a required password change.
- `403 Forbidden` — access denied, permission denied, or the actor could not be
  resolved.

See [Cross-Cutting Concerns](cross-cutting.md#error-handling) for the full
mapping and response shape.
