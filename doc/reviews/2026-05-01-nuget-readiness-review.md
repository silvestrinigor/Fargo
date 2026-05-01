# Fargo SDK NuGet Readiness Review

Date: 2026-05-01

Scope:
- `src/Fargo.Sdk`
- `src/Fargo.Sdk.Contracts`

Goal:
- Prepare both projects for a first public alpha/beta/test NuGet publish.

## Summary

The SDK is close enough to package, but it is not ready for a clean first public publish yet. `Fargo.Sdk` can currently produce a `.nupkg`; `Fargo.Sdk.Contracts` can also pack, but with weak default metadata. A Release solution build currently fails in `Fargo.Api`, which is a release-pipeline blocker even though the package project can be packed individually.

Recommended publish order:
1. Fix Release build.
2. Add explicit package metadata to both package projects.
3. Add a real license or keep packages private/unlisted.
4. Create package-specific README files.
5. Decide what is public API versus internal implementation.
6. Add symbol/source metadata and package validation.
7. Pack and test installation from a local feed before pushing to NuGet.

## Blockers

### 1. Release build fails

`dotnet build Fargo.slnx -c Release --no-restore` fails:

```text
src/Fargo.Api/Contracts/ContractMappings.cs(192,28): error CS0104:
'TreeNodeType' is an ambiguous reference between
'Fargo.Application.Tree.TreeNodeType' and 'Fargo.Sdk.Contracts.Tree.TreeNodeType'
```

Fix:
- Fully qualify the contract enum in `EntityTreeNode` mapping:
  `Fargo.Sdk.Contracts.Tree.TreeNodeType`.
- Consider aliases for all duplicated boundary enum names to avoid future ambiguity.

### 2. `Fargo.Sdk.Contracts` package metadata is incomplete

The generated nuspec currently contains:

```xml
<id>Fargo.Sdk.Contracts</id>
<description>Package Description</description>
```

It is also missing:
- `PackageReadmeFile`
- `PackageTags`
- license metadata
- repository URL/project URL
- package icon, if wanted
- release notes

This package is the wire-contract package. It needs very clear metadata because consumers may install it directly.

### 3. No public license

The root README says:

```text
This project is currently under development and does not yet have a public license.
All rights reserved.
```

Do not publicly publish to NuGet until the license is decided. If this is only for internal testing, use a private feed or publish as unlisted and still set clear metadata.

### 4. The NuGet README is the repository README

`Fargo.Sdk` packs the root `README.md`. That README describes the whole server, Web UI, Aspire, database, DocFX, and says the project has no license. It is not a good package landing page.

Fix:
- Add `src/Fargo.Sdk/README.md` for SDK usage.
- Add `src/Fargo.Sdk.Contracts/README.md` for contract DTO usage.
- Pack each project-local README.

## High Priority Cleanup

### 5. Public API surface is too broad

The package exposes many implementation-level types:
- `FargoHttpClient`
- `FargoSdkHttpClient`
- `FargoSdkHttpResponse<TData>`
- `FargoProblemDetails`
- low-level `*HttpClient` interfaces and implementations
- older public `*Client` classes with internal constructors

For a first package, decide whether the supported surface is:
- high-level `Engine` and managers/services only; or
- both high-level services and low-level HTTP clients.

If low-level HTTP clients are not intended for direct consumers, make them `internal` or document them as advanced/unstable. Otherwise they become part of the public compatibility contract.

### 6. XML documentation warnings are extensive

Both publishable projects generate XML docs, but many public members lack comments. This makes IntelliSense look unfinished and hides important usage details.

Fix:
- Either complete XML docs for public API before publish; or
- temporarily disable `GenerateDocumentationFile` for alpha packages, though completing docs is preferred.

Important areas:
- public enums in `Fargo.Sdk.Contracts`
- `Fargo.Sdk` public clients
- `Engine`
- `FargoSdkResponse<T>`
- exception types
- DI extension methods

### 7. Targeting only `net10.0` narrows installability

Both packages target only `net10.0`. That may be intentional, but it means only .NET 10 consumers can install them.

Options:
- Keep `net10.0` for now if Fargo itself is .NET 10-only.
- Consider `netstandard2.1` or `net8.0` for `Fargo.Sdk.Contracts` because it only contains DTOs/enums.
- Consider `net8.0` or `net9.0` for `Fargo.Sdk` if dependencies and API usage allow it.

For a beta, being explicit in the README is enough. For public adoption, wider targeting helps.

### 8. Source/debug package support is missing

The generated `.nupkg` includes DLL and XML docs, but no `.snupkg` symbol package.

Recommended project properties:
- `IncludeSymbols=true`
- `SymbolPackageFormat=snupkg`
- `PublishRepositoryUrl=true`
- Source Link package/configuration if repository URL is public

NuGet.org supports `.snupkg` symbol packages for debugging.

### 9. Package versions are build-derived alpha numbers

MinVer currently produces versions like:
- `Fargo.Sdk.0.0.0-alpha.0.433`
- `Fargo.Sdk.Contracts.0.0.0-alpha.0.434`

This is acceptable for internal tests, but awkward for public alpha. Before the first public push, choose a deliberate version such as:
- `0.1.0-alpha.1`
- `0.1.0-beta.1`

Make sure `Fargo.Sdk` and `Fargo.Sdk.Contracts` publish with the same version.

## Medium Priority Cleanup

### 10. `Fargo.Sdk` depends on `Fargo.Sdk.Contracts` as a package

This is correct for package publishing, but both packages must be pushed together in dependency order:
1. `Fargo.Sdk.Contracts`
2. `Fargo.Sdk`

For a local feed test, verify a clean sample app can install `Fargo.Sdk` and restore `Fargo.Sdk.Contracts` transitively.

### 11. Package dependencies are current but exposed

`Fargo.Sdk` exposes dependencies on:
- `Microsoft.AspNetCore.SignalR.Client`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Http`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Options`

These are reasonable, but the public API should avoid leaking more dependency types than necessary. The DI extension necessarily exposes `IServiceCollection`; the rest should stay isolated.

### 12. Public exception constructors are mostly internal

Types like `FargoSdkApiException`, `FargoSdkConnectionException`, `InvalidCredentialsFargoSdkException`, and `PasswordChangeRequiredFargoSdkException` are public, but most useful constructors are internal.

This can be okay if consumers only catch these exceptions, but it limits testing and manual creation. Decide intentionally:
- public catch-only exceptions; or
- public constructors for testability and custom adapters.

### 13. Contract package naming is mostly good, but DTO stability needs discipline

`Fargo.Sdk.Contracts` uses DTO/request names like `ArticleDto`, `ArticleCreateRequest`, `AuthDto`. That is a good direction.

Before publish:
- Avoid renaming DTOs after first public release unless versioning with breaking changes.
- Decide whether enum numeric values are stable forever.
- Add tests that serialize representative contracts and compare JSON shape.

### 14. Old generated/cache artifacts are ignored, but present locally

There are `.lscache` files under SDK project folders. They are ignored by `.gitignore`, so they should not be committed. Keep an eye on package contents when adding broad `None Include` rules.

## Suggested First Publish Checklist

1. Fix Release build ambiguity in `ContractMappings.cs`.
2. Add `Directory.Packages.props` or centralized package/package metadata if desired.
3. Add package metadata to both csproj files:
   - `PackageId`
   - `Title`
   - `Description`
   - `PackageTags`
   - `PackageReadmeFile`
   - `PackageLicenseExpression` or `PackageLicenseFile`
   - `RepositoryUrl`
   - `PackageProjectUrl`
   - `PackageReleaseNotes`
4. Add project-local README files.
5. Decide license before public NuGet.
6. Add symbol/source package settings.
7. Decide public API surface and hide implementation-only types.
8. Resolve XML doc warnings on public API.
9. Add contract JSON-shape tests.
10. Run:

```bash
dotnet build Fargo.slnx -c Release --no-restore
dotnet test Fargo.slnx -c Release --no-restore
dotnet pack src/Fargo.Sdk.Contracts/Fargo.Sdk.Contracts.csproj -c Release -o artifacts/packages
dotnet pack src/Fargo.Sdk/Fargo.Sdk.csproj -c Release -o artifacts/packages
```

11. Install both packages from `artifacts/packages` into a clean sample project.
12. Only then push to NuGet or a private feed.

## Local Checks Performed

Commands run:

```bash
dotnet pack src/Fargo.Sdk/Fargo.Sdk.csproj --no-build -c Release -o /tmp/fargo-pack-check
dotnet build Fargo.slnx -c Release --no-restore
dotnet pack src/Fargo.Sdk.Contracts/Fargo.Sdk.Contracts.csproj --no-build -c Release -o /tmp/fargo-pack-check
```

Observed:
- `Fargo.Sdk` packed successfully.
- `Fargo.Sdk.Contracts` packed after Release artifact existed, but emitted missing README warning.
- Release solution build failed due to `TreeNodeType` ambiguity.
- Existing package vulnerability warnings remain in non-SDK projects.

