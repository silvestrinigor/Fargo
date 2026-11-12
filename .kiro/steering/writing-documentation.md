---
inclusion: fileMatch
fileMatchPattern: "docs/**/*.md"
---

# Writing documentation (docfx site under docs/)

Write what the reader needs, nothing they get faster elsewhere. The full guide
is `docs/contributing/writing-documentation.md`; follow it when writing or
editing pages under `docs/`.

- **Placement:** `concepts/` is for users (no code internals), `server/` for
  server developers, `cli/` `web-api/` `mcp/` for client users (overview,
  getting started, reference), `contributing/` for contributors. One page owns
  each topic; others link to it instead of repeating it.
- **Never hand-edit generated content:** `cli/reference/*.md`
  (`scripts/generate-cli-docs.sh`, and add new command groups to `CLI_GROUPS`
  there), `web-api/reference/fargo.swagger.json`
  (`scripts/generate-rest-api-docs.sh`), and `api/` (built from the XML docs).
- **New page:** a lowercase-hyphenated file in its section folder, an entry in
  `docs/manual/toc.yml` (paths relative to it, like `../server/x.md`), and a
  link from the section overview.
- **Structure:** one `#` heading in Title Case matching `toc.yml`, an opening
  paragraph, then `##` sections in sentence case. Path pages end with
  `## Next steps`.
- **Style:** short, plain, present tense, US English. Bold a term where it is
  defined, code formatting for literals, tables for settings
  (`Setting | Default | Meaning`), numbered lists for ordered steps. Wrap prose
  at about 80 characters. Update the docs in the same change as the behaviour,
  and make stated defaults match the code.
- **Links:** relative paths between pages (`../server/realtime.md#anchor`), `~/`
  for generated references (`~/api/index.md`,
  `~/web-api/reference/fargo.swagger.json`), and `xref:` with the full
  namespace for .NET types.
- **Code blocks:** always the correct language (`bash`, `powershell`, `csharp`,
  `json`, `http`, `text` for output), commands run from the repo root,
  `<placeholder>` values, never secrets.
- **Diagrams:** Mermaid. Use `> [!NOTE]`-style alerts rarely. Images go in
  `docs/img/` with alt text.
- **Verify:** `docfx docs/docfx.json` and fix page warnings (`InvalidBookmark`,
  `InvalidFileLink`, bad xrefs). Delete stale `docs/api/*.yml` first (keep
  `api/index.md`), or they hide broken xrefs that fail in CI.
