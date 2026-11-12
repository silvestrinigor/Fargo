---
inclusion: fileMatch
fileMatchPattern: "**/*.cs"
---

# C# code documentation

Document only what the signature cannot tell you. The full guide, with
examples, is `docs/contributing/code-documentation.md`; follow it when writing
or editing C# code.

## Keep (short)

- A one-sentence type `<summary>` when the name does not say what the type is for.
- Behaviour a caller could get wrong: invariants, side effects, ordering,
  concurrency, security rules, what `null` or an empty result means, units.
- Why something is done, in `<remarks>`, when it is not obvious (a security
  reason, a race, a workaround).
- Short summaries on the public surface of the client libraries
  (`Fargo.ClientHttp`, `Fargo.ClientAuthentication`, `Fargo.ClientRealtime`,
  `Fargo.RealtimeContracts`), which other developers see in IntelliSense.
- `<exception>` only for domain exceptions a caller is expected to catch, and
  never on command or query handlers (errors are mapped to HTTP centrally).
- Inline `//` comments that explain why, a workaround or a non-obvious decision.

## Remove

- Summaries that restate the member name: properties, constants, constructors,
  operators, `Equals`, `GetHashCode`, `ToString`.
- `<param>` and `<returns>` that restate the name or type, and every
  `cancellationToken` parameter tag.
- `<inheritdoc/>` on trivial overrides.
- Generic `<remarks>` that would fit any class.
- XML docs on private and internal members, unless they explain why.
- Inline `//` comments that narrate what the next line obviously does.

## Format

- Keep the three-line form for summaries:

  ```csharp
  /// <summary>
  /// One sentence.
  /// </summary>
  ```

- An Entity Framework constructor gets a plain `// Required by EF Core.`
  comment, not an XML doc block.
- Never edit `Generated/` (Kiota) or `Migrations/` files.
