using Fargo.Infrastructure.Outbox;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Testcontainers.PostgreSql;

namespace Fargo.Infrastructure.Tests.Common;

/// <summary>
/// Starts a throwaway PostgreSQL container (Docker required) with the outbox
/// table created, for tests that need real PostgreSQL behavior such as raw SQL,
/// row locks, or <c>ExecuteDelete</c>.
/// </summary>
/// <remarks>
/// Only the <c>AddOutboxMessages</c> migration is applied, by running its own
/// operations. The full schema cannot be built on an empty database today: the
/// migration chain fails in <c>IntroduceEntityBaseTable</c> (cannot drop
/// <c>pk_user_groups</c>), and <c>EnsureCreated</c> fails on duplicate
/// <c>pk_entities</c> constraint names across the TPT tables.
/// </remarks>
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string OutboxMigrationSuffix = "_AddOutboxMessages";

    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    public string ConnectionString => container.GetConnectionString();

    /// <summary>
    /// Creates a context configured like the application's, including the
    /// outbox interceptor.
    /// </summary>
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

        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var outboxMigration = migrationsAssembly.Migrations
            .Single(m => m.Key.EndsWith(OutboxMigrationSuffix, StringComparison.Ordinal));
        var migration = migrationsAssembly.CreateMigration(outboxMigration.Value, context.Database.ProviderName!);

        var commands = context.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations);

        foreach (var command in commands)
        {
            await context.Database.ExecuteSqlRawAsync(command.CommandText);
        }
    }

    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}
