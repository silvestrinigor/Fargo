using Fargo.Infrastructure.Outbox;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Fargo.Infrastructure.Tests.Common;

/// <summary>
/// Starts a throwaway PostgreSQL container (Docker required) with the full
/// schema migrated, for tests that need real PostgreSQL behavior such as raw
/// SQL, row locks, or <c>ExecuteDelete</c>.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    public string ConnectionString => container.GetConnectionString();

    public FargoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseNpgsql(ConnectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new OutboxSaveChangesInterceptor(TimeProvider.System))
            .Options;

        return new FargoDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await container.StartAsync();

        await using var context = CreateContext();

        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}
