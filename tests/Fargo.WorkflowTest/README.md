# Fargo.WorkflowTest

An end-to-end **workflow driver** for the Fargo system. It talks to a running
Fargo HTTP API through the real `Fargo.ClientHttp` client and runs two scenarios.

## Inventory scenario

1. **Authenticate** as the seeded admin user.
2. **Create container articles + items** (articles of `ArticleType.Container`,
   each with one concrete container item).
3. **Create product articles**, each with several concrete items.
4. **Place** every item into a starting container (round-robin).
5. **Relocate** items between containers with a seeded sequence of random moves
   (`PATCH /v1/items/{guid}` with `ParentItemContainerGuid`).
6. **Remove** a subset of items from all containers
   (`RemoveFromParentItemContainer = true`).
7. **Verify** the result against the API: each item's server-side parent
   container, movement-history length, the location chain, and per-container
   occupancy.

## Identity & authorization scenario

Runs after the inventory scenario (toggle with `Workflow:RunIdentityScenarios`).
Every created name is suffixed with a per-run tag so repeated runs against the
same database do not collide. See `IdentityScenario.cs`.

8.  **Partitions** — create a hierarchy (Warehouse > Zone-A, Zone-B), verify the
    parent link, and edit a partition (`PATCH`).
9.  **User groups** — create an `inventory-operators` group with specific
    permissions (`CreateArticle`, `CreateItem`, `EditItem`) and access to Zone-A.
10. **Users** — create an operator (member of the group, inheriting its
    permissions and partition access) and a limited user (Zone-B access, no
    permissions). New users are created password-change-required, so the driver
    clears that via `PUT /v1/identity/password` before logging in as them.
11. **Permission mutation** — grant then revoke a permission on the group
    (`PermissionsToAdd` / `PermissionsToRemove`) and on a user directly, verifying
    each change via `GET`.
12. **Permission enforcement** — log in AS the operator and assert it *can*
    create articles/items in Zone-A but is *denied* (403) creating users, user
    groups, and partitions; log in AS the limited user and assert every write is
    denied.
13. **Partition-access scoping** — the operator can read Zone-A (has access) but
    reading Zone-B returns 404 (outside its access set).
14. **Deletes** — the admin deletes the users, group, and partitions, exercising
    every delete endpoint.

## Inventory calculation scenario

Runs after the identity scenario (toggle with `Workflow:RunInventoryScenarios`).
It builds a known containment layout and asserts every computed inventory number
against the expected value. See `InventoryScenario.cs`.

Layout (all items are of one entity-tracked article):

```
Shelf (container)              <- 2 items directly
  +-- Tote (nested container)  <- 3 items directly
Pallet (container)             <- 1 item directly
```

15. **Article inventory** (`GET /v1/articles/{guid}/inventory`): total = 6;
    Pallet direct = 1; Shelf direct-only (`includeDescendents=false`) = 2; Shelf
    including the nested Tote (`includeDescendents=true`, the default) = 5;
    Shelf + Pallet together = 6.
16. **Container inventory** (`GET /v1/items/{guid}/inventory`): Shelf reports 5
    of the article (recursive — the HTTP endpoint always includes descendants),
    Tote 3, Pallet 1; after emptying the Pallet it reports no row for the article
    (zero-net entries are dropped).
17. **Point-in-time** (`at`): after moving Tote's 3 items to the Pallet, the live
    inventory shows Tote = 0 / Pallet = 4, but as-of a pre-move snapshot Tote = 3
    and the article total = 6 (moves never change the total).
18. **Event-tracked path guard** (`POST /v1/item/events`): recording a movement
    or creation event against an *entity-tracked* article is correctly rejected
    (400 — "not event-tracked"); listing events (requires `ReadItemEvents`)
    succeeds and returns none for that article.

Each assertion is logged individually; the process exits non-zero if any
inventory verification or authorization check fails.

## Article property coverage scenario

Toggle with `Workflow:RunArticlePropertyScenarios`. Creates articles with every
optional property populated and across every article type, reading each back and
asserting the values round-trip. See `ArticlePropertiesScenario.cs`.

19. **Fully-populated Default article**: name, description, shelf life, color,
    mass, all three dimensions (X/Y/Z), a generated valid EAN-13 barcode, and the
    entity tracking mode — every value asserted on read-back.
20. **Typed articles**: a Variation (references its base article), a Pack
    (base article + quantity 12), a Kit (component + quantity), and a Container
    (with a max-mass limit), each verified by type and payload.
21. **Event-tracked article**: created with `TrackingMode = Event` and confirmed
    via the article's `TrackingMode` on read-back (the API capability added for
    this workflow).

## Event-tracked item-events scenario

Toggle with `Workflow:RunItemEventScenarios`. Creates an event-tracked article
and two containers, records creation/movement/deletion events, and asserts the
computed event-based inventory. See `ItemEventScenario.cs`.

Movement plan (article total on the right):

```
M1 receipt   null -> A   +100   A=100            total=100
M2 transfer  A -> B        40   A=60,  B=40       total=100 (transfer nets 0)
M3 dispatch  B -> null     10   A=60,  B=30       total=90
```

22. **Computed inventory**: article total = 90 (100 received − 10 dispatched),
    Container A net = 60, Container B net = 30 — asserted through both the article
    inventory and the container inventory endpoints.
23. **Lifecycle events do not count**: a Creation (+500) and a Deletion (−20) are
    recorded but leave the computed inventory unchanged (the aggregation reads
    only movement events). The point-in-time inventory before any movement is 0,
    and after the receipt is 100.
24. **Event listing & filters**: the article has 5 events total and 3 movement
    events when filtered by `eventType`.

Each assertion is logged individually; the process exits non-zero if any check
fails.

This is an internal driver, not a shipped artifact. It is not packaged and has
no independent version.

## Prerequisites

A running Fargo API with a migrated + seeded database. The easiest way is the
Aspire AppHost, which starts PostgreSQL, runs migrations, seeds data and the
API:

```powershell
dotnet run --project src/server/Fargo.AppHost
```

Note the API URL from the Aspire dashboard (or run `Fargo.ServiceHttp`
standalone against your own PostgreSQL and point `Workflow:Server` at it).

## Run

```powershell
# Build once
dotnet build tests/client/Fargo.WorkflowTest

# Run (all settings come from appsettings.json)
dotnet run --project tests/client/Fargo.WorkflowTest
```

The process exits `0` when every verification assertion passes, non-zero
otherwise.

## Configuration

All configuration lives in **`appsettings.json`** under the `Workflow` section —
not in code:

```json
{
  "Workflow": {
    "Server": "http://localhost:5534",
    "Username": "admin",
    "Password": "Senha123@",
    "Containers": 3,
    "Articles": 5,
    "ItemsPerArticle": 4,
    "Moves": 20,
    "Seed": 1234
  }
}
```

| Key               | Meaning                                       |
| ----------------- | --------------------------------------------- |
| `Server`          | API base address                              |
| `Username`        | Login name                                    |
| `Password`        | Login password                                |
| `Containers`      | Number of container items (min 2)             |
| `Articles`        | Number of product articles (min 1)            |
| `ItemsPerArticle` | Items created per product article (min 1)     |
| `Moves`           | Random relocations after placement (>= 0)     |
| `Seed`            | RNG seed (reproducible move sequence)         |
| `NewUserPassword` | Password assigned to users the workflow creates (must satisfy the server policy) |
| `RunTag`          | Suffix appended to generated names; empty = timestamp-based |
| `RunIdentityScenarios` | When `true`, also run the identity/authorization scenario |
| `RunInventoryScenarios` | When `true`, also run the inventory calculation scenario |
| `RunArticlePropertyScenarios` | When `true`, also run the article-property coverage scenario |
| `RunItemEventScenarios` | When `true`, also run the event-tracked item-events scenario |

The settings are bound and **validated at startup** (`ValidateOnStart`); a
missing or invalid value fails the run with a clear message.

### Overriding without editing the file

Standard .NET configuration layering applies, in increasing precedence:
`appsettings.json` → `appsettings.{Environment}.json` → environment variables →
command-line arguments. So you can override any value per run:

```powershell
# Environment variable (double underscore = section separator)
$env:Workflow__Moves = 50
dotnet run --project tests/client/Fargo.WorkflowTest

# Command-line argument
dotnet run --project tests/client/Fargo.WorkflowTest -- --Workflow:Server=http://localhost:7563
```

## How it drives the API

The driver uses only the generated `FargoApiClient` surface:

- `client.V1.Identity.Login.PostAsync(IdentityLoginDto)` — authenticate; the
  returned token is stored in the `ITokenStore` so the bearer handler attaches
  it to every later request automatically.
- `client.V1.Articles.PostAsync(ArticleCreateDto)` — create articles
  (`ArticleType` 5 = Container, 1 = Default).
- `client.V1.Items.PostAsync(ItemCreateDto)` — create items.
- `client.V1.Items[guid].PatchAsync(ItemUpdateDto)` — **the move operation**
  (set `ParentItemContainerGuid` to place, `RemoveFromParentItemContainer` to
  take out).
- `client.V1.Items[guid].GetAsync()` / `.Moviments` / `.Location` — verification.
