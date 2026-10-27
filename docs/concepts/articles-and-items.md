# Articles and Items

Fargo separates *what a product is* from *the actual physical units* of it. This
is the single most important distinction in the catalog.

## Articles — the definition

An **article** is a product definition: the description shared by every unit of
that product. It carries attributes such as a name and description, optional
barcodes, physical dimensions, mass, color, and shelf life. An article does not
represent anything physical on a shelf — it is the template.

Articles come in a few **types**, which capture how one article relates to
others:

| Article type | What it represents |
| --- | --- |
| **Default** | A plain product definition. |
| **Variation** | A variant that derives from another article (e.g. a different color of the same product). |
| **Pack** | A bundle of a given quantity of another article (e.g. a box of 12). |
| **Kit** | A composite made up of several component articles in specific quantities. |
| **Container** | An article whose items can hold other items (a bin, shelf, pallet, …). |

Variations, packs, and kits reference the article(s) they are built from, which
lets the catalog express relationships between products without duplicating
their definitions.

## Items — the physical units

An **item** is a concrete, physical instance of an article — the thing actually
sitting in the warehouse. If an article is "Widget, blue," an item is "this
particular blue widget." Many items can share one article.

An item knows which article it instantiates and where it currently lives.

## Tracking mode — entity vs event

Not every kind of stock is best represented as individually numbered units.
Each article therefore chooses a **tracking mode** that decides how its stock is
recorded. The mode is fixed for the article's lifetime.

### Entity tracking (the default)

With **entity tracking**, every physical unit is a distinct **item**. This is
the model described above: each unit has its own identity, its own location, its
own movement history, and can be individually fixed in place. Article inventory
is simply the number of item rows.

Use entity tracking when you control and care about each unit — serialized,
high-value, or otherwise individually managed goods — and you have (or want to
maintain) complete information for each one.

### Event tracking

With **event tracking**, no per-unit items exist at all. Instead, stock is
recorded as a stream of **movement events**, each describing that a *quantity* of
the article moved between containers (or into/out of stock). The current stock is
*derived* from those events rather than stored as rows.

Use event tracking when tracking each unit individually is impractical or
unnecessary, for example:

- **No single-unit control.** The goods are fungible or bulk — screws, sand,
  liquids, grain — where there is no meaningful "one unit" to identify and move.
- **Incomplete per-item information.** You do not have, or do not want to
  maintain, full details for each individual unit; you only know *how much*
  moved and *where*.
- **Very high volume or throughput.** The number of units, or the rate of
  movements, is so large that creating and moving one item row per unit would be
  impractical. Recording an aggregate quantity per event is far cheaper.

A movement event carries a positive **quantity**, an optional **origin**
container, an optional **destination** container, and the **instant** it
occurred (which may be back-dated, e.g. when importing historical data). A
movement with only a destination is a *receipt* into stock; one with only an
origin is a *dispatch* out of stock; one with both is a *transfer*.

> Entity-tracked and event-tracked articles coexist in the same catalog and the
> same containers. A container's inventory lists both kinds side by side.


## Containers and location

An item whose article is of type **Container** can hold other items. Because a
container is itself an item, containers can nest: a pallet item can contain box
items, which contain individual product items. This forms an item
**containment hierarchy**.

At any moment an item is either loose (not inside anything) or placed inside
exactly one container item. Its **location** is the chain of containers leading
up to it — for example `Shelf A3 › Box 7 › this item`.

A few rules keep the hierarchy sane:

- An item cannot be its own container.
- An item can only be placed inside an item whose article is a container.
- A **fixed** item cannot be moved or removed from its container until it is
  unfixed.
- An item cannot be placed inside one of its own descendants (that would create
  a cycle).

## Movement and history

Whenever an item changes containers, the move is recorded. Fargo keeps a
**containment history** so you can ask not only "where is this item now?" but
also "where was this item at a given point in time?" — the location can be
reconstructed for any past instant.

## Inventory

Because items reference articles and live in containers, Fargo can answer
inventory questions:

- **Article inventory** — how many physical units of a given article exist,
  optionally limited to specific containers (and, optionally, everything nested
  inside them).
- **Container inventory** — what a container holds, counted per article.

Both can also be evaluated **as of a past instant**, reconstructed from the
containment history.

### How event-tracked stock is counted

For an event-tracked article there are no item rows to count. Its inventory is
instead the **net of signed movement quantities**: the total quantity moved
*into* a container minus the total quantity moved *out* of it. A transfer
between two containers that are both inside the queried scope nets to zero, as
expected. Because every event is timestamped, an event-based inventory can also
be evaluated as of a past instant by considering only the events that occurred
at or before it — exactly like the point-in-time view of entity-tracked stock.
Event-derived quantities may be fractional (for bulk goods), whereas
entity-tracked counts are always whole numbers of items.


## Partitions apply here too

Every item and article belongs to one or more [partitions](partitions.md), which
determine who can see and change it. Creating, moving, or deleting an item
requires both the right [permission](permissions.md) and partition access to the
item.
