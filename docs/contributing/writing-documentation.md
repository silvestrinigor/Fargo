# Writing Documentation

The documentation site is built with [docfx](https://dotnet.github.io/docfx/)
from the Markdown pages under `docs/`. The same rule applies as for
[code documentation](code-documentation.md): write what the reader needs to
know, and nothing the reader can get faster from somewhere else.

## Where a page goes

| Section | Folder | Reader | Contains |
|---|---|---|---|
| Concepts | `concepts/` | Anyone using Fargo | What the domain ideas mean and the rules users run into. No class names or code internals. |
| Server | `server/` | Developers working on the server | How the server is built: layers, persistence, security, operations. |
| CLI, Web API, MCP | `cli/`, `web-api/`, `mcp/` | Users of that client | An `overview.md`, a `getting-started.md`, and a reference. |
| Contributing | `contributing/` | Contributors | How to work on Fargo itself, like this page. |

Each topic has one page that owns it. Other pages link to that page instead of
repeating it. For example, real-time push is explained in
`server/realtime.md`, and the CLI overview only covers `fargo realtime watch`
and links there.

## Generated content: never edit by hand

| Content | Source | Regenerate with |
|---|---|---|
| `cli/reference/*.md` | The CLI's `--help` output | `bash scripts/generate-cli-docs.sh` |
| `web-api/reference/fargo.swagger.json` | The API's OpenAPI document | `bash scripts/generate-rest-api-docs.sh` |
| `api/` (the .NET API reference) | The XML documentation in `src/` | `docfx docs/docfx.json` (metadata step) |

- **A new CLI command group:** add it to the `CLI_GROUPS` list in
  `scripts/generate-cli-docs.sh`. Otherwise it gets no reference page.
- **Pages in the .NET API reference:** these come from the
  [code documentation](code-documentation.md). Improve the XML comments, not
  the generated files.
- **Stale API files:** docfx never deletes the `api/*.yml` files of types that
  were renamed or moved. A local build can still resolve links to types that no
  longer exist, while the CI build, from a fresh checkout, breaks them without
  a warning. Delete `docs/api/*.yml` (keep `api/index.md`) before checking a
  local build.

## Adding a page

1. Put it in the folder of its section, named in lowercase with hyphens, for
   example `server/realtime.md`.
2. Add it to `docs/manual/toc.yml`, the sidebar for all the manual pages. Use
   paths relative to that file (`../server/realtime.md`). A page that isn't
   listed there can't be found from the navigation. The top bar
   (`docs/toc.yml`) only separates the manual from the generated API
   reference, so you don't edit it.
3. Link to it from the section's overview page, and from the pages that cover
   neighboring topics.

## Page structure

- **One `#` heading** per page, in Title Case, matching its name in
  `toc.yml`: `# Getting Started`.
- **An opening paragraph** that says what the page covers and for whom, before
  the first section.
- **`##` sections** in sentence case: `## Access inheritance`. Go one level
  deeper (`###`) only when a section really has parts.
- **Pages that are part of a path** (overview, then getting started, then
  reference) end with a short `## Next steps` list of links.

## Writing style

- **Plain and direct.** Short sentences, present tense, US English. Address the
  reader as "you" in how-to pages.
- **Explain, don't restate.** Say what a setting does and when to change it, not
  only that it exists. Don't describe code line by line; link to the type in
  the API reference instead.
- **Bold** a term where it is defined (`A **partition** is…`), once per page.
  Use *italics* rarely, for emphasis.
- **Code formatting** for anything the reader types or sees literally: commands,
  paths, type and member names, configuration keys, HTTP routes.
- **Tables** for facts with the same shape, such as settings
  (`Setting | Default | Meaning`), options or comparisons.
- **Numbered lists** for steps that happen in order, bullets for everything else.
- **Wrap prose at about 80 characters.** Tables, long links and code blocks are
  exempt.
- **Keep it true.** A change that alters behaviour, a default or a command
  updates the docs in the same change. A default stated in the docs must match
  the code.

## Links

| To | Form | Example |
|---|---|---|
| Another page | Relative path, with an optional anchor | `[Partitions](../concepts/partitions.md#access-inheritance)` |
| A heading on the same page | Anchor | `[below](#links)` |
| The generated references | Path from the docs root, with `~/` | `[.NET API Reference](~/api/index.md)`, `[Web API Reference](~/web-api/reference/fargo.swagger.json)` |
| A .NET type | `xref:` with the full namespace | `` [`ActionType`](xref:Fargo.Core.Entities.Actors.ActionType) `` |
| An external site | Full `https://` URL | `[.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)` |

An anchor is the heading in lowercase, with spaces turned into hyphens and
punctuation dropped: `## Watching live changes` becomes `#watching-live-changes`.
Renaming a heading breaks the links to it, so search for its anchor first.

## Code blocks

- **Always name the language**, and make it the right one: `bash`,
  `powershell`, `csharp`, `json`, `http`. Use `text` for terminal output and
  help text.
- **Commands run from the repository root**, and are complete enough to copy.
- **Placeholders** use angle brackets: `<password>`, `<guid>`. Never put real
  secrets in the docs, not even development ones.

## Diagrams, notes and images

- **Diagrams:** use Mermaid (a `mermaid` code block), which the site renders.
  Keep diagrams small enough to read without zooming.
- **Notes:** `> [!NOTE]`, `> [!TIP]` and `> [!WARNING]` blocks are for
  information the reader must not miss. Use them rarely; when every page has
  three of them, readers skip them all.
- **Images:** prefer text, tables or diagrams. When a picture is needed, store
  it in `docs/img/` and give it alt text that says what it shows.

## Checking the site

Build the site locally from the repository root:

```bash
docfx docs/docfx.json
```

Fix every warning about the pages, such as `InvalidBookmark` (an anchor that no
longer exists), `InvalidFileLink` and invalid cross-references. Warnings about
NuGet packages or projects come from the solution, not from the docs. The output
goes to `docs/_site/`, which is not committed.

The last step, generating the PDF, downloads a headless Chromium browser. It
fails without internet access or behind a proxy that intercepts certificates;
the site itself is built before that step.

## Next steps

- [Code Documentation](code-documentation.md): the rules for XML comments,
  which feed the .NET API reference.
