# Permissions

A **permission** authorizes a specific *kind of action*. Permissions decide what
an actor may *do*; [partitions](partitions.md) decide what data the action may
touch. An operation is allowed only when the actor holds the required permission
**and** has partition access to the affected data.

## The catalog of actions

Each permission corresponds to one action. Most resources have a
create / delete / edit triple, plus a few targeted actions:

| Area | Permissions |
| --- | --- |
| Articles | create, delete, edit |
| Items | create, delete, edit |
| Users | create, delete, edit, change another user's password |
| User groups | create, delete, edit, change members |
| Partitions | create, delete, edit |
| Audit logs | read |
| Item events | read |

Some actions are deliberately separated from the basic edit permission because
they are more sensitive — for example, changing another user's password or
changing a group's membership are distinct from editing the user or group's
other fields, and reading audit logs is its own permission rather than being
implied by any edit right. Reading item events — the stream of stock changes
recorded for event-tracked articles — is likewise a distinct read permission.

## Direct and inherited grants

A permission can be granted:

- **directly to a user**, or
- **to a user group** the user belongs to (and is inherited through
  [nested groups](users-and-groups.md#nesting-groups)).

A user's **effective permissions** are the union of all of these. There is no
"deny" — access is additive. Removing a permission means removing every grant
(direct and via groups) that provides it.

## How a permission is enforced

Before changing state, a handler checks the required permission against the
resolved actor. If the actor lacks it, the request is rejected with
**`403 Forbidden`**. For example, creating a partition requires the
"create partition" permission; without it the operation never runs.

Granting a permission is itself gated: an actor can only grant a permission it
already holds. This prevents privilege escalation — you cannot hand out access
you do not have yourself.

## Permission is necessary but not sufficient

Holding a permission is only half of an authorization decision. The actor must
*also* have [partition access](partitions.md) to the specific entity involved.

- A user with "delete items" but no access to Warehouse B cannot delete an item
  in Warehouse B.
- A user with access to Warehouse B but without "delete items" cannot delete
  items there either.

Both checks must pass. See [Users and User Groups](users-and-groups.md#how-a-users-effective-access-is-composed)
for how an actor accumulates both permissions and partition access.
