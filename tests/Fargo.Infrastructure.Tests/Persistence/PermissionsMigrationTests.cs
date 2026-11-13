using Fargo.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fargo.Infrastructure.Tests.Persistence;

/// <summary>
/// The migration that moved permissions from jsonb columns into join tables was edited by hand to copy the
/// data, so both directions are checked against a database that already holds grants.
/// </summary>
public sealed class PermissionsMigrationTests : IAsyncLifetime
{
    private const string BeforeMigration = "20261112214950_AddIdempotencyKeys";

    private static readonly Guid ActorGuid = Guid.Parse("6f2b0c1e-0000-4000-8000-000000000001");

    private static readonly Guid UserGroupGuid = Guid.Parse("6f2b0c1e-0000-4000-8000-000000000002");

    private readonly PostgresFixture postgres = new();

    public Task InitializeAsync() => postgres.StartWithoutMigratingAsync();

    public Task DisposeAsync() => postgres.DisposeAsync();

    [Fact]
    public async Task Migration_ShouldMovePermissionsIntoTheJoinTablesAndBack()
    {
        await using var context = postgres.CreateContext();
        var migrator = context.GetService<IMigrator>();

        await migrator.MigrateAsync(BeforeMigration);

        // The interpolated values are this test's own GUID constants.
#pragma warning disable EF1002
        await context.Database.ExecuteSqlRawAsync(
            $"""
            INSERT INTO entities (guid, entity_type) VALUES ('{ActorGuid}', 2), ('{UserGroupGuid}', 3);

            INSERT INTO actors (guid, actor_type, nameid, is_active, description, permissions)
            VALUES ('{ActorGuid}', 1, 'migration-actor', true, '', '[1, 5, 5]');

            INSERT INTO user_groups (guid, is_active, nameid, description, permissions)
            VALUES ('{UserGroupGuid}', true, 'migration-group', '', '[2, 3]');
            """);
#pragma warning restore EF1002

        await migrator.MigrateAsync();

        Assert.Equal([1, 5], await QueryIntsAsync(context,
            $"SELECT action_type AS \"Value\" FROM actor_permissions WHERE actor_guid = '{ActorGuid}' ORDER BY action_type"));
        Assert.Equal([2, 3], await QueryIntsAsync(context,
            $"SELECT action_type AS \"Value\" FROM user_group_permissions WHERE user_group_guid = '{UserGroupGuid}' ORDER BY action_type"));

        await migrator.MigrateAsync(BeforeMigration);

        Assert.Equal([1, 5], await QueryIntsAsync(context,
            $"SELECT value::integer AS \"Value\" FROM actors, jsonb_array_elements_text(permissions) WHERE guid = '{ActorGuid}' ORDER BY 1"));
        Assert.Equal([2, 3], await QueryIntsAsync(context,
            $"SELECT value::integer AS \"Value\" FROM user_groups, jsonb_array_elements_text(permissions) WHERE guid = '{UserGroupGuid}' ORDER BY 1"));
    }

    private static async Task<List<int>> QueryIntsAsync(DbContext context, string sql)
        => await context.Database.SqlQueryRaw<int>(sql).ToListAsync();
}