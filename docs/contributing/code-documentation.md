# Code documentation

XML documentation comments (`///`) are for what the signature cannot tell you.
A comment that repeats the name of a member costs reading time and drifts out of
date without adding anything. Write fewer comments, and make each one count.

## The rule

Before writing a comment, ask: *would a caller who reads only the signature get
something wrong?* If not, leave the comment out.

## What to document

| Keep | Example |
|---|---|
| A one-sentence type summary, when the name does not say what the type is for | `OutboxMessage`: "An integration event waiting to be published, stored in the same transaction as the change that produced it." |
| Behaviour a caller could get wrong | `SecurityPrincipal.HasAccess`: an entity with no partitions is accessible. |
| Concurrency, ordering, side effects | `FargoTokenRefresher`: only one refresh at a time, because the API revokes a refresh token when it is used. |
| What `null`, an empty result or a unit means, when it is not obvious | "`null` while the message is still waiting to be published." |
| Why something is done, in `<remarks>` | "Disposing a broken channel can throw; there is nothing to recover." |
| Domain exceptions a caller is expected to catch | `<exception cref="FargoCoreException">` on a domain method that guards an invariant. |
| Short summaries on the public client libraries | `Fargo.ClientHttp`, `Fargo.ClientAuthentication`, `Fargo.ClientRealtime`, `Fargo.RealtimeContracts` are used by other developers, who see the summaries in IntelliSense. |

Inline `//` comments follow the same rule: keep the ones that explain why, a
workaround or a non-obvious decision.

## What not to document

| Remove | Instead of |
|---|---|
| Summaries that restate the name | `/// Gets the name string value.` on `Name.Value`; `/// Maximum length.` on `MaxLength`; `/// Initializes a new instance.` |
| `<param>` and `<returns>` that restate the name or type | `/// <param name="articleGuid">The unique identifier of the article.</param>` |
| Every `cancellationToken` parameter tag | `/// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>` |
| Operators and standard overrides | `/// Determines whether two names are equal.` on `operator ==`; `<inheritdoc/>` on `Equals`, `GetHashCode`, `ToString` |
| `<exception>` on command and query handlers | Errors are turned into HTTP responses centrally, so the list adds nothing for callers. |
| Generic remarks that would fit any class | `/// Changes are typically committed through a unit of work.` |
| XML docs on private and internal members | Unless they explain why. |
| Inline comments that narrate the code | `// Save the user` above `await repository.SaveAsync(user);` |

## Format

Keep the three-line form for summaries:

```csharp
/// <summary>
/// One sentence.
/// </summary>
```

An Entity Framework constructor gets a plain comment instead of an XML doc
block, because it explains why the private constructor exists:

```csharp
// Required by EF Core.
private Article() { }
```

Generated code (`Generated/`, Kiota) and EF Core migrations (`Migrations/`) are
never edited by hand.

## Before and after

```csharp
// Before
/// <summary>
/// Gets an article by its unique identifier.
/// </summary>
/// <param name="articleGuid">
/// The unique identifier of the article.
/// </param>
/// <param name="cancellationToken">
/// A token used to cancel the asynchronous operation.
/// </param>
/// <returns>
/// The matching <see cref="Article"/> if found; otherwise,
/// <see langword="null"/>.
/// </returns>
Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default);

// After: the signature already says all of it.
Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default);
```

Missing-documentation warnings (CS1591) are turned off in `.editorconfig`, so
leaving a member undocumented is a deliberate, supported choice.
