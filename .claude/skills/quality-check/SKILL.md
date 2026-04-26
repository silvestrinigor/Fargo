---
name: quality-check
description: Reminds to write unit tests and validate XML documentation for every change.
---

# Quality Check

Before marking any coding task as complete, verify both items below.

## 1 — Unit Tests

Every new class or significant new behavior must have at least one corresponding `[Fact]` in `Fargo.Sdk.Tests`.

Follow the three-layer pattern already established:

| Layer | File pattern | What to test |
|---|---|---|
| Entity | `tst/.../Articles/ArticleTests.cs` | Property getters/setters, event wiring |
| Service | `tst/.../Articles/ArticleManagerTests.cs` | Business-logic delegation, error propagation |
| HTTP client | `tst/.../Articles/ArticleClientTests.cs` | HTTP response → SDK error type mapping |

Run before completing:

```bash
dotnet test tst/Fargo.Sdk.Tests/ --filter "Category!=Integration"
```

All unit tests must pass without a running server.

## 2 — XML Documentation

Every new or modified `public` or `internal` type and member must have complete `///` XML doc comments:
- `<summary>` — always required
- `<param>` — for every parameter
- `<returns>` — when the method returns a meaningful value
- `<exception>` — for documented exceptions
- `<remarks>` — for non-obvious constraints or behaviour

Build warnings **CS1591** (missing XML documentation) must remain at zero for all touched files.

Run to verify:

```bash
dotnet build
```

Zero warnings is the bar. If the build shows CS1591, add the missing docs before closing the task.
