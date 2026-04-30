# First-Pass Review

Open items kept from the initial review while the event persistence path is being fixed:

1. Article image storage is not coordinated with database commits.
   Files:
   `src/Fargo.Application/Articles/ArticleImageUploadCommand.cs`
   `src/Fargo.Application/Articles/ArticleImageDeleteCommand.cs`
   `src/Fargo.Application/Articles/ArticleDeleteCommand.cs`

2. API key enforcement can block `/health` and break Aspire readiness checks when `EnforceApiClient` is enabled.
   Files:
   `src/Fargo.Api/Middlewares/ApiClientMiddleware.cs`
   `src/Fargo.ServiceDefaults/Extensions.cs`
   `src/Fargo.AppHost/AppHost.cs`

3. Event query authorization likely uses the wrong permission.
   File:
   `src/Fargo.Application/Events/EventManyQuery.cs`

4. `Fargo.Application.Tests` and `Fargo.Infrastructure.Tests` were present but effectively empty during the review.
   Files:
   `tst/Fargo.Application.Tests/Fargo.Application.Tests.csproj`
   `tst/Fargo.Infrastructure.Tests/Fargo.Infrastructure.Tests.csproj`

5. Package vulnerability warnings were reported during restore and test.
   Files:
   `src/Fargo.ServiceDefaults/Fargo.ServiceDefaults.csproj`
   transitive dependencies pulling `System.Security.Cryptography.Xml`
