# Fargo - Feature Gaps / Issues to Fix Later

> Internal backlog. Fargo today is a **partition-scoped catalog + physical-containment ledger**
> (strong "what is where / what was where" engine, two tracking modes, point-in-time history, audit
> log, clean CQRS/Aspire platform). It is **not yet a commercial WMS**. Each item below is written as a
> standalone issue so it can be picked up independently. Assessment grounded in server code
> (Fargo.Core, Fargo.Application, Fargo.Infrastructure, Fargo.ServiceHttp).

---

## Business / domain

### [ ] GAP-01 - Order / transaction flows (Tier 1)
No purchase orders, sales/outbound orders, receiving, put-away, picking, packing, or shipping. You can
only create/move/delete items directly. This is the single biggest gap for real use.

### [ ] GAP-02 - Reservations / allocations / available-to-promise (Tier 1)
`TotalCount` is raw on-hand only; two clients can both claim the same unit. No on-hand vs available vs
allocated concept.

### [ ] GAP-03 - Replenishment signals (Tier 2)
No reorder points, min/max levels, safety stock, or low-stock thresholds; system can't say *when* to
restock.

### [ ] GAP-04 - Lot / batch / serial / expiry tracking (Tier 1)
`ShelfLife` is article-level metadata only; no per-unit expiry date, lot number, or batch. Blocks FEFO
for food/pharma. Entity GUIDs are not managed serial numbers.

### [ ] GAP-05 - Stock adjustments / cycle counting / reconciliation (Tier 1)
Corrections are hard deletes or raw events, not counted-and-approved adjustments with reason codes
(expected vs actual).

### [ ] GAP-06 - Suppliers / customers (Tier 2)
No trading-partner entities to attach to receipts/shipments.

### [ ] GAP-07 - Cost / valuation / pricing (Tier 3)
No money concept, no unit cost, no FIFO/LIFO/average cost, no currency. Can't answer "what is this
inventory worth?"

### [ ] GAP-08 - Real locations + capacity enforcement (Tier 2)
Partitions are authorization boundaries, not physical bins/zones. `ArticleContainer.MaxMass` is defined
but NEVER checked on placement (PlaceInsideContainer / event movement validation ignore it). No
volume/slot/count capacity, no pick paths. (Enforcing MaxMass is a quick win - see below.)

### [ ] GAP-09 - Kit / assembly execution + returns (Tier 3)
Kits/packs are definitions only; no assembly execution (consume components to build a kit) or explosion
back into components. No returns/RMA flow.

### [ ] GAP-10 - Unit-of-measure conversion (Tier 3)
UnitsNet carries a unit but values are stored as-given; no conversion between purchase/stock/sell units
(each/box/pallet).

---

## Platform / non-functional

### [x] GAP-11 - Optimistic concurrency control (Tier 1)
No row-version / `xmin` tokens on any aggregate; risk of silent lost updates under concurrent edits (the
simulation concurrency test confirmed "last writer wins"). (Quick win - see below.)

### [ ] GAP-12 - Soft delete / retention / archival (Tier 2)
Deleting an aggregate physically removes the row (history survives only via temporal tables + audit
logs). No undo / compliance archival.

### [ ] GAP-13 - Enterprise auth (Tier 3, partially resolved / scoped)
Status after review (2026-10):

- **Account lockout - DONE (was a false positive).** `AuthenticationAttempt` is not just tracked; the
  login handler calls `IAuthenticationAttemptService.IsAllowedAsync` BEFORE verifying credentials and
  enforces three thresholds (>=5 fails per IP+identifier / 5 min; >=20 per IP / 1 min; >=5 per identifier
  / 15 min), returns the longest `RetryAfter`, records every failure, and surfaces 429 with `retryAfter`.
  Success resets the count. No work needed.
- **MFA - WON'T DO (by design).** Fargo runs internally over a VPN on shared warehouse terminals, not on
  the public web. Per-login MFA would wreck floor throughput, and the VPN perimeter is itself a factor.
  Documented risk acceptance: if an attacker is already on the VPN there is no second barrier.
- **OAuth/OIDC/SSO - OPTIONAL.** Only worth it if operators should log in with a corporate directory
  (AD / Entra / LDAP) for centralized account lifecycle. Skip while Fargo's own user store is the
  intended home for accounts. (Open question: does the org have a central directory?)
- **Service-to-service auth - REAL, build when the first machine caller appears.** Design captured in
  the appendix below (native `ServiceAccount` + `ActorType.Service` + client-credentials token endpoint).
  Not yet built because no confirmed unattended machine caller exists today.
- **JWT signing-key hardening - REAL, low priority given VPN.** Today a single shared symmetric secret
  (`JwtOptions.Key`, HMAC-SHA256) both signs and verifies, so anyone who can read it can forge an admin
  token, and there is no rotation (changing the key logs everyone out). Fix tiers: (a) treat the secret
  as a real secret - out of source/logs, long/random, injected via a secrets manager; (b) add rotation
  support - multiple keys + `kid` header + an accept-set so keys can change with zero forced logouts;
  (c) optionally switch to asymmetric (RSA/ECDSA) signing so verifiers only ever get the public key -
  worth it once more than one service must validate Fargo tokens.

### [ ] GAP-14 - Outbound integration / eventing (Tier 3)
No webhooks, message bus, or working outbox/inbox (the `ComInbox` type is orphaned, not mapped to the
DB). Nothing can notify an ERP/carrier when stock changes.

### [ ] GAP-15 - Bulk/batch endpoints + idempotency keys (Tier 2)
One HTTP call per item (throttled by the 100/min per-user rate limit); a retried "receive" can
double-count. No idempotency keys.

### [ ] GAP-16 - Query capability: sorting + search (Tier 2/3)
Pagination exists and filtering is endpoint-specific, but there is NO sorting anywhere and no general
search. No real-time (SignalR) for live dashboards.

### [ ] GAP-17 - Caching, background jobs/scheduler, async long-running ops (Tier 3)
No reorder-check job, no report generation, no 202-Accepted + poll pattern. DB `EnableRetryOnFailure` is
set for migrations only, not the runtime DbContext.

### [ ] GAP-18 - CI integration tests (Tier 2)
Good domain/handler unit tests exist, but E2E drivers (WorkflowTest, SimulationWorker) need a live
instance; no automated integration tests with a throwaway DB (Testcontainers / WebApplicationFactory).

---

## Suggested prioritization

- **Tier 1 (blocks real operations):** GAP-01, GAP-02, GAP-04, GAP-05, GAP-11
- **Tier 2 (operational maturity):** GAP-03, GAP-06, GAP-08, GAP-12, GAP-15, GAP-18
- **Tier 3 (integration & enterprise):** GAP-07, GAP-09, GAP-10, GAP-13, GAP-14, GAP-16, GAP-17

## Quick wins (small, self-contained, high value)

- [ ] Enforce `ArticleContainer.MaxMass` on placement (confirmed: the field exists and is NOT checked in
  `Item.PlaceInsideContainer` nor in the event-movement validation). -> GAP-08
- [x] Add an EF concurrency token (`xmin`) to the mutable aggregates (Article, Item, User, UserGroup,
  Partition) + translate `DbUpdateConcurrencyException` to 409. Done in code; EF migration still to be
  generated/reviewed. -> GAP-11
- [x] ~~Wire account lockout to `AuthenticationAttempt`~~ - already implemented; removed as a false
  positive. -> GAP-13

---

## Appendix A - Service-to-service auth design (GAP-13)

Ready-to-implement plan. Build when the first confirmed unattended machine caller appears (e.g. an ERP
importer, a scheduled job, or the MCP server running under its own identity rather than a human token).

### Why this shape

Fargo's authorization is built around an `Actor` (GUID + `ActorType` + permission set + partition-access
set). `ActorType` is an enum that today has only `User = 1`, and `ActorResolver` switches on it - so the
model already anticipates non-user actors. A service should therefore be a first-class actor, not a fake
user. Mechanism: the slim OAuth2 client-credentials pattern (client id + secret -> short-lived JWT),
which reuses Fargo's existing JWT validation and `Actor` pipeline wholesale.

Deliberately NOT mTLS/client certificates (operationally heavy; overkill on a trusted VPN) and NOT an
external IdP (only relevant if also doing SSO).

### Components

1. **`ServiceAccount` aggregate (new, Fargo.Core).** Fields: `Guid`, human-readable `ClientId`
   (unique, like `Nameid`), a HASHED client secret (reuse `IPasswordHasher`/`ITokenHasher` - never store
   clear), `IsActive`, its own permissions (`ActionType` set) and partition accesses (mirroring `User`),
   and an `AuthVersion` for revocation (same trick users use).
2. **`ActorType.Service = 2`** + teach `ActorResolver.GetActorByGuidAndTypeAsync` to resolve it
   (load the `ServiceAccount`, pull permissions + partition accesses, build
   `Actor(guid, ActorType.Service, perms, partitions)`). ~10 lines following `GetUserActorByGuidAsync`.
   Nothing downstream changes - `ThrowIfPermissionDenied` / `ThrowIfAccessDenied` already take an `Actor`.
3. **Token endpoint** `POST /v1/identity/service-token` with `{ clientId, clientSecret }`: look up by
   clientId, constant-time verify the secret hash, check `IsActive`, issue a JWT. Same shape as a user
   token but with an `actor_type: service` claim and `sub = serviceGuid`; shorter lifetime (e.g. 15 min).
   Reuse `JwtTokenGenerator` almost verbatim.
4. **Make `CurrentUserActor` read the `actor_type` claim** instead of hard-coding `ActorType.User`
   (`service` -> Service, default -> User so existing user tokens are unchanged). Rename to `CurrentActor`.
5. **Extend `UseFargoTokenValidation`** with a parallel branch for service tokens (re-check the
   `ServiceAccount` is active + `AuthVersion` matches on each request), mirroring the user check.
6. **Management endpoints** to create / disable / rotate-secret a `ServiceAccount`, each permission-gated;
   `ServiceAccount` itself should be an auditable `EntityType`.

### Flow

```
service -> POST /v1/identity/service-token {clientId, clientSecret}
        <- { accessToken (JWT: actor_type=service, sub=serviceGuid), expiresAt }
service -> GET /v1/items  Authorization: Bearer <serviceToken>
        -> JwtBearer validates signature/issuer/audience/lifetime (existing path)
        -> CurrentActor reads actor_type=service, sub=guid
        -> ActorResolver resolves Service actor (perms + partitions)
        -> existing permission/partition checks apply; audit log attributes to ActorType.Service
```

### Caveats

- The client secret is a bearer credential - same risk class as the JWT signing key (GAP-13 above).
  Store only the hash, one secret per service, support per-service rotation (issue new while old still
  valid briefly).
- Adding `ActorType.Service` ripples into audit-log queries / any actor-type filtering, and likely a new
  `EntityType.ServiceAccount`.
- Not a quick win: new aggregate + migration + repository + token endpoint + command handlers +
  `ActorResolver`/`CurrentActor` changes + validation hook + management endpoints. Tier-2 feature.