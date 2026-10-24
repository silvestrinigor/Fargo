# Audit Logs

An **audit log** is an immutable record of a change to the system: who did what,
to which entity, and when. Fargo writes an audit log for every create, update,
and delete, so the history of the data is always attributable and recoverable.

## What is recorded

Each audit log captures:

- the **actor** that performed the action (its identifier and type),
- the **affected entity** and its **type** (article, item, user, user group,
  partition, …),
- the **kind of change** — created, updated, or deleted,
- the **time** the change occurred, and
- a **metadata** bag describing the specifics of the change.

Audit logs are never modified or deleted through normal operations — they are an
append-only trail.

## Metadata

The metadata bag is an open, typed set of values that records the meaningful
details of a change so it can be understood later without guessing. Values can
be strings, numbers, booleans, null, nested objects, or arrays — enough to
capture things like which fields changed, which partitions or permissions were
added or removed, which container an item moved into, or that a password was
changed (the password value itself is never recorded).

The goal is that an audit entry carries enough context to answer "what exactly
did the actor change?" on its own.

## Access control: audit visibility follows the entity

This is the key rule for audit logs:

> **An audit log is visible to exactly the actors who can see the entity it
> describes.**

An audit log does not get its own independently assigned partitions. Instead, it
**inherits the partition scope of the audited entity** at the time the change
happened. So:

- An audit log for an article in Warehouse A is scoped to the same partitions as
  that article, and is therefore visible only to actors with
  [access](partitions.md) to Warehouse A (or an ancestor that grants it).
- For a **partition** action, the audit log is associated with the affected
  partition's **parent** partition (or the global partition when the affected
  partition has no parent), because that is the scope from which the change is
  observable.

The practical consequence: a user confined to one branch sees audit history only
for the data in that branch. They cannot read the audit trail of entities in
partitions they have no access to — the same boundary that governs the data
governs its history.

## Reading audit logs

Reading audit logs requires the dedicated **read audit logs**
[permission](permissions.md) — it is not implied by any edit right. On top of
that, the usual [partition](partitions.md) filtering applies, so results are
limited to audit logs whose (inherited) partitions the actor can access.

Audit logs can be listed and filtered — for example by actor, by affected
entity, by entity type, or by time period — always within the actor's visible
scope.
