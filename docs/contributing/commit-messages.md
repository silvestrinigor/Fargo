# Commit Messages

Fargo uses [Conventional Commits](https://www.conventionalcommits.org/). A
commit message tells the reader what changed and why, so the history can be
scanned, searched, and turned into release notes.

## Format

```text
<type>(<scope>): <subject>

<body>

<footer>
```

Only the first line is required.

## Subject line

- Use the imperative mood: "add partition commands", not "added" or "adds".
- Start with a lowercase letter and leave out the trailing period.
- Keep it to 72 characters or fewer, and say what the change does.
- Do not use vague subjects such as `code cleanup` or `metadatas`.

## Types

| Type | Use for |
|---|---|
| `feat` | New behavior that users or API clients can see. |
| `fix` | A bug fix. |
| `docs` | Documentation only, including XML comments and `docs/` pages. |
| `refactor` | A code change that does not alter behavior. |
| `perf` | A change that improves performance. |
| `test` | Adding or fixing tests. |
| `style` | Formatting only, such as `dotnet format`. |
| `build` | Build, packages, and dependencies. |
| `ci` | CI and workflow configuration. |
| `chore` | Anything else that does not touch behavior. |

## Scopes

The scope is optional. When used, it names the area changed.

| Scope | Area |
|---|---|
| `core` | `Fargo.Core` |
| `application` | `Fargo.Application` |
| `infrastructure` | `Fargo.Infrastructure` |
| `http` | `Fargo.ServiceHttp` and `Fargo.ClientHttp` |
| `realtime` | `Fargo.ServiceRealtime`, `Fargo.ClientRealtime`, and `Fargo.RealtimeContracts` |
| `cli` | `Fargo.Cli` |
| `mcp` | `Fargo.Mcp` |
| `migration` | `Fargo.ServiceMigration` |
| `docs` | The `docs/` site |

Services without their own scope (`Fargo.ServiceInitializer`,
`Fargo.ServiceMaintenance`, `Fargo.ServiceEventPublisher`, `Fargo.AppHost`,
`Fargo.ServiceDefaults`) use the name without the `Fargo.` prefix, in
lowercase, for example `initializer`. Omit the scope when a change spans
several areas.

## Body

Add a body when the reason is not obvious from the subject.

- Separate it from the subject with a blank line.
- Wrap lines at about 72 characters.
- Explain why the change was made and what it affects. The diff already shows
  how.

## Footer

- Mark a breaking change with `!` after the type or scope, and add a
  `BREAKING CHANGE:` footer that says what to do instead.
- Reference issues with `Closes #123` or `Refs #123`.

## One logical change per commit

- Keep one logical change in each commit.
- Do not mix formatting (`dotnet format`) or renames with behavior changes.
  Put them in their own `style` or `refactor` commit.
- Update the documentation in the same commit as the behavior it describes, as
  the [writing documentation](writing-documentation.md) guide requires.
- Do not commit generated files that were edited by hand.

## Examples

Good:

```text
feat(application): add partition permission commands
```

```text
fix(infrastructure): retry outbox publishing after a transient failure

The publisher stopped after the first exception, so events stayed
unpublished until the service restarted.
```

```text
feat(http)!: require an idempotency key on create requests

BREAKING CHANGE: POST endpoints return 400 without an Idempotency-Key header.
```

Bad:

```text
code cleanup
```

```text
Fixed stuff.
```

## Merges

Work happens on branches from `develop`. Merge commits that Git generates
(`Merge branch 'develop' of ...`) are fine and need no convention.
