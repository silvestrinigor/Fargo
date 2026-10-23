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

## Partitions apply here too

Every item and article belongs to one or more [partitions](partitions.md), which
determine who can see and change it. Creating, moving, or deleting an item
requires both the right [permission](permissions.md) and partition access to the
item.
