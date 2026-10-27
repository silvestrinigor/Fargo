# Security

Fargo authenticates callers with JWT bearer tokens and authorizes them with a
combination of **permissions** and **partition access**, resolved into an
`Actor`. Every state change is recorded in an audit log.

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

See the [Web API Overview](~/docs/web-api/overview.md#authentication) for the
HTTP-level details.

## The actor

Requests act on behalf of an `Actor` — the authenticated identity together with
its resolved **permissions** and **partition access**. The current actor is
provided by `ICurrentActor` (`CurrentUserActor`), and `ActorResolver` computes
the actor's effective permissions and accessible partitions, aggregating what is
granted directly to the user and what is inherited through the user's group
hierarchy.

Application handlers resolve the actor and enforce authorization before touching
domain state.

## Permissions

Permissions are values of [`ActionType`](xref:Fargo.Core.Actors.ActionType) — a
create / delete / edit permission per resource plus a few specific actions such
as `ChangeAnotherUserPassword`, `ChangeUserGroupMembers`, `ReadAuditLogs`, and
`ReadItemEvents`.

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
// Fargo.Core.Actors.Actor
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
