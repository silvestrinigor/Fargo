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

### [ ] GAP-11 - Optimistic concurrency control (Tier 1)
No row-version / `xmin` tokens on any aggregate; risk of silent lost updates under concurrent edits (the
simulation concurrency test confirmed "last writer wins"). (Quick win - see below.)

### [ ] GAP-12 - Soft delete / retention / archival (Tier 2)
Deleting an aggregate physically removes the row (history survives only via temporal tables + audit
logs). No undo / compliance archival.

### [ ] GAP-13 - Enterprise auth (Tier 3)
No MFA, no API keys, no OAuth/OIDC/SSO, no service-to-service auth; single shared symmetric JWT secret
(no asymmetric signing / key rotation); `AuthenticationAttempt` is tracked but no account-lockout policy
is wired to it. (Wiring lockout is a quick win - see below.)

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

- [ ] Enforce `ArticleContainer.MaxMass` on placement (field exists; check is missing). -> GAP-08
- [ ] Add an EF concurrency token (row-version) to aggregates. -> GAP-11
- [ ] Wire account lockout to the existing `AuthenticationAttempt` tracking. -> GAP-13
