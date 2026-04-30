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

6. `FargoSession` subscribes to authentication events but does not unsubscribe on dispose, which can accumulate duplicate notifications in longer-lived Blazor Server circuits.
   File:
   `src/Fargo.Web.Components/Session/FargoSession.cs`

7. MCP tools report failures as successful string payloads like `Error: ...` instead of surfacing protocol-level tool errors.
   Files:
   `src/Fargo.Mcp/Tools/ArticleTools.cs`
   `src/Fargo.Mcp/Tools/ItemTools.cs`
   `src/Fargo.Mcp/Tools/PartitionTools.cs`
   `src/Fargo.Mcp/Tools/UserTools.cs`

8. MCP tools parse user-supplied GUIDs inline with `Guid.Parse(...)`, so validation errors are opaque and folded into the generic error-string path.
   Files:
   `src/Fargo.Mcp/Tools/ArticleTools.cs`
   `src/Fargo.Mcp/Tools/ItemTools.cs`
   `src/Fargo.Mcp/Tools/PartitionTools.cs`
   `src/Fargo.Mcp/Tools/UserTools.cs`
