using Microsoft.Extensions.Logging;

namespace Fargo.Api.Tests.Integration;

/// <summary>
/// Integration tests for Partition CRUD operations. Requires a running Fargo API.
/// Tests are automatically skipped when the server is unreachable.
/// To run: dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
public sealed class PartitionIntegrationTests : IClassFixture<ServerAvailabilityFixture>
{
    private const string Server = "https://localhost:7563";
    private const string ValidUser = "admin";
    private const string ValidPassword = "HJLBaQLIcinDp6KrqRjjgQ@";

    private readonly ServerAvailabilityFixture serverFixture;

    public PartitionIntegrationTests(ServerAvailabilityFixture serverFixture)
    {
        this.serverFixture = serverFixture;
    }

    private void SkipIfUnavailable() =>
        Skip.If(!serverFixture.IsAvailable, $"Fargo API is not running at {Server}");

    private static Engine CreateEngine() =>
        new(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace)));

    [SkippableFact]
    public async Task CreateAsync_Should_ReturnPartition_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);

        var partition = await engine.Partitions.CreateAsync("Integration Test Partition");

        Assert.NotEqual(Guid.Empty, partition.Guid);
        Assert.Equal("Integration Test Partition", partition.Name);

        await engine.Partitions.DeleteAsync(partition.Guid);
    }

    [SkippableFact]
    public async Task GetAsync_Should_ReturnPartition_When_PartitionExists()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var created = await engine.Partitions.CreateAsync("Integration Test Partition Get");

        var partition = await engine.Partitions.GetAsync(created.Guid);

        Assert.Equal(created.Guid, partition.Guid);
        Assert.Equal("Integration Test Partition Get", partition.Name);

        await engine.Partitions.DeleteAsync(partition.Guid);
    }

    [SkippableFact]
    public async Task GetManyAsync_Should_ReturnPartitions_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var created = await engine.Partitions.CreateAsync("Integration Test Partition List");

        var partitions = await engine.Partitions.GetManyAsync();

        Assert.NotEmpty(partitions);

        await engine.Partitions.DeleteAsync(created.Guid);
    }

    [SkippableFact]
    public async Task UpdateAsync_Should_UpdatePartition_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var partition = await engine.Partitions.CreateAsync("Integration Test Partition Update");

        await partition.UpdateAsync(p => p.Name = "Integration Test Partition Updated");

        Assert.Equal("Integration Test Partition Updated", partition.Name);

        await engine.Partitions.DeleteAsync(partition.Guid);
    }

    [SkippableFact]
    public async Task DeleteAsync_Should_DeletePartition_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var partition = await engine.Partitions.CreateAsync("Integration Test Partition Delete");

        await engine.Partitions.DeleteAsync(partition.Guid);

        var exception = await Record.ExceptionAsync(() => engine.Partitions.GetAsync(partition.Guid));
        Assert.IsAssignableFrom<FargoSdkApiException>(exception);
    }
}
