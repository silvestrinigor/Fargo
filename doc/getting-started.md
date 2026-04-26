# Getting Started

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) — used by .NET Aspire to run SQL Server locally

## Running the Application

Clone the repository and start the full stack with .NET Aspire:

```bash
git clone <repository-url>
cd Fargo
dotnet run --project src/Fargo.AppHost
```

Aspire starts SQL Server in a Docker container, runs the migration and seed services, then launches the API and Blazor web frontend. The Aspire dashboard URL is printed in the terminal — open it to see the URLs for each service.

## Running Tests

```bash
dotnet test
```

## Using the SDK

`Fargo.Sdk` is the typed .NET client library for the Fargo API. Add a project reference:

```xml
<ProjectReference Include="..\..\src\Fargo.Sdk\Fargo.Sdk.csproj" />
```

### Engine Pattern (scripts, desktop apps, non-DI scenarios)

`Engine` is the manual composition root. Create one instance per application, call `LogInAsync`, and dispose it on shutdown.

```csharp
using Fargo.Sdk;

using var engine = new Engine();

// Authenticate and connect the real-time hub
await engine.LogInAsync("https://localhost:7001", "admin", "P@ssword1");

// List all articles
var articles = await engine.Articles.GetManyAsync();
foreach (var a in articles)
    Console.WriteLine($"{a.Guid}  {a.Name}");

// Create an article
var widget = await engine.Articles.CreateAsync("Widget A", description: "The first widget");

// Subscribe to real-time updates on this specific entity
widget.Updated += (_, e) => Console.WriteLine($"Widget updated: {e.Guid}");
widget.Deleted += (_, e) => Console.WriteLine("Widget was deleted");

// Modify and persist the change
await widget.UpdateAsync(a => a.Name = "Widget A (revised)");

// Dispose the entity when done to unsubscribe from hub events
await widget.DisposeAsync();

await engine.LogOutAsync();
```

### Dependency Injection (Blazor, ASP.NET Core, Worker Services)

Register the SDK in `Program.cs`:

```csharp
builder.Services.AddFargoSdk(opt =>
{
    opt.Server = "https://localhost:7001";
});
```

Inject the manager interfaces where needed:

```csharp
public class ArticleListViewModel(IArticleManager articles, IAuthenticationService auth)
{
    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (!auth.IsAuthenticated)
            await auth.LogInAsync("admin", "P@ssword1", ct);

        Items = await articles.GetManyAsync(ct: ct);
    }

    public IReadOnlyCollection<Article>? Items { get; private set; }
}
```

#### Available Manager Interfaces

| Interface | Domain |
|-----------|--------|
| `IArticleManager` | Articles, images, barcodes, partition assignments |
| `IItemManager` | Items and partition assignments |
| `IPartitionManager` | Partition hierarchy |
| `IUserManager` | Users, partition access, group membership |
| `IUserGroupManager` | User groups and shared permissions |
| `IApiClientManager` | API clients and key management |
| `IAuthenticationService` | Login, logout, token refresh, password change |

### Session Persistence

Implement `ISessionStore` to persist the session across restarts (useful in Blazor Server):

```csharp
builder.Services.AddFargoSdk(opt => opt.Server = "https://localhost:7001")
                .WithSessionStore<MyBrowserSessionStore>();
```

On startup, call `engine.RestoreSessionAsync(server)` (Engine pattern) or `auth.RestoreAsync()` (DI pattern) to resume the previous session without re-authenticating.

### API Key Authentication

For machine clients, set `FargoSdkOptions.ApiKey` instead of logging in:

```csharp
builder.Services.AddFargoSdk(opt =>
{
    opt.Server = "https://localhost:7001";
    opt.ApiKey = "your-api-key";
});
```

The key is sent as the `X-Api-Key` header on every request.

### Checking Permissions

```csharp
if (auth.Session.HasActionPermission(ActionType.CreateArticle))
{
    var article = await articles.CreateAsync("New product");
}

if (auth.Session.HasPartitionAccess(partitionGuid))
{
    // user can see this partition
}
```

## Temporal Queries

Most service methods accept a `DateTimeOffset? temporalAsOf` parameter to query the state of entities at a point in the past:

```csharp
var snapshot = await engine.Articles.GetManyAsync(temporalAsOf: DateTimeOffset.UtcNow.AddDays(-7));
```
