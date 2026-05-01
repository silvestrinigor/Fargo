# Fargo Code Design Review

Date: 2026-05-01

This note captures the main design issues found during the code review so they are not lost.

## Current Health

- `dotnet test Fargo.slnx --no-restore` passed.
- Results observed: 382 passed, 28 skipped, 0 failed.
- The build/test output still contains many warnings.
- A high-severity vulnerable transitive package warning was reported for `System.Security.Cryptography.Xml`.

## Findings

1. API contracts are mixed with Application and Domain models.
   - API endpoints expose Application models such as `ArticleInformation` and receive Application commands directly.
   - Application request/response models contain Domain value objects such as `Name` and `Description`.
   - SDK models and payloads are separately hand-shaped, which creates contract drift risk.
   - Recommendation: make `Fargo.Sdk.Contracts` the real HTTP DTO contract layer and map DTOs to Application commands inside the API.

2. Authorization and partition filtering are repeated across handlers.
   - `ArticleManyQuery`, `UserManyQuery`, `ItemManyQuery`, and similar handlers repeat the same admin/system/public/partition branching.
   - This is security-sensitive duplication.
   - Recommendation: introduce a reusable access-scope/filter abstraction, such as `ActorAccessScope`, plus shared query helpers.

3. Application handler registration lives in Infrastructure.
   - `Fargo.Infrastructure.Extensions.ServiceCollectionExtensions` manually registers Application command/query handlers.
   - This makes Infrastructure responsible for Application composition and creates a maintenance risk when handlers are added.
   - Recommendation: move registration to `Fargo.Application` or use scanning with explicit conventions.

4. Events are manually recorded and published.
   - Mutation handlers manually call `eventRecorder.Record`, `unitOfWork.SaveChanges`, and `eventPublisher.Publish...`.
   - Some workflows, such as article image upload/delete, mutate article state without obviously recording/publishing article-updated events.
   - Recommendation: use an Application pipeline/decorator or domain event collector so audit/event behavior cannot be forgotten. Consider an outbox if reliable delivery matters.

5. Image storage is only partially coordinated with database state.
   - Upload compensates when DB save fails, but previous-image cleanup can still fail after commit.
   - Delete saves DB state first, then deletes the file, which can leave orphaned files if file deletion fails.
   - Local storage accepts `image/svg+xml`; this should be a deliberate security decision.
   - Recommendation: treat cleanup as retryable background work, validate size/content, and exclude SVG unless required.

6. Event query authorization likely uses the wrong permission.
   - `EventManyQueryHandler` gates event history behind `ActionType.EditApiClient`.
   - Recommendation: add a dedicated permission such as `ViewEvents` or `ViewAuditLog`, or make it explicitly admin-only.

7. API key enforcement can block health endpoints.
   - `ApiClientMiddleware` exempts `/authentication` but not `/health` or `/alive`.
   - When API key enforcement is enabled, Aspire readiness/liveness checks can fail.
   - Recommendation: explicitly exempt `/health`, `/alive`, and any intentional unauthenticated development endpoints.

8. Dependency and build hygiene need tightening.
   - `Fargo.Application` references EF Core even though the observed usage appears limited to expression trees.
   - `Fargo.Infrastructure` references old ASP.NET packages such as `Microsoft.AspNetCore.Identity` and `Microsoft.AspNetCore.Mvc` version `2.3.9`.
   - `Directory.Build.props` uses floating MinVer version `7.*`.
   - Recommendation: remove unused package references, replace old ASP.NET packages with modern/necessary references, pin versions, and consider central package management.

9. Warning noise is high.
   - Many public XML documentation warnings are emitted.
   - Important compiler/security warnings are harder to notice.
   - Recommendation: either complete the docs for public/package surfaces or suppress XML doc warnings for internal projects. Consider treating selected warning categories as errors after cleanup.

10. Existing review items still worth tracking.
    - `FargoSession` subscribes to authentication events but `Dispose` does not unsubscribe.
    - MCP tools return error strings such as `Error: ...` instead of surfacing protocol-level tool errors.
    - MCP tools parse GUIDs inline with `Guid.Parse`, making validation errors opaque.

## Suggested Fix Order

1. Fix immediate security/runtime risks:
   - vulnerable package warning,
   - event query permission,
   - health endpoint API-key exemption.

2. Stabilize API contracts:
   - move HTTP DTOs into `Fargo.Sdk.Contracts`,
   - map contracts to Application models inside API endpoints,
   - adjust SDK to use contract types instead of anonymous payloads where possible.

3. Centralize authorization and partition access:
   - create reusable access-scope/query helper abstractions,
   - add focused tests around public/no-partition/admin/regular-user behavior.

4. Centralize mutation side effects:
   - introduce command pipeline/decorators or domain events,
   - ensure event recording and SignalR publishing are consistently covered.

5. Improve side-effect reliability:
   - make image cleanup retryable,
   - validate image uploads,
   - add tests for DB/file failure scenarios.

6. Reduce warning noise:
   - remove unused references,
   - pin package versions,
   - decide XML documentation policy per project.
