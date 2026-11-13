using Fargo.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fargo.Infrastructure.Tests.Persistence;

/// <summary>
/// The migration that merged the movement and lifecycle payload tables into <c>item_events</c> was edited
/// by hand to copy the data, so both directions are checked against a database that already holds events.
/// </summary>
public sealed class UnifyItemEventsMigrationTests : IAsyncLifetime
{
    private const string BeforeMigration = "20261112223710_AddEnumLookupsAndNormalizePermissions";

    private static readonly Guid WidgetArticle = Guid.Parse("7a1c0000-0000-4000-8000-00000000000a");
    private static readonly Guid BoxArticle = Guid.Parse("7a1c0000-0000-4000-8000-00000000000b");
    private static readonly Guid SandArticle = Guid.Parse("7a1c0000-0000-4000-8000-00000000000c");
    private static readonly Guid BoxOne = Guid.Parse("7a1c0000-0000-4000-8000-000000000001");
    private static readonly Guid BoxTwo = Guid.Parse("7a1c0000-0000-4000-8000-000000000002");
    private static readonly Guid Widget = Guid.Parse("7a1c0000-0000-4000-8000-000000000003");
    private static readonly Guid ReceiptEvent = Guid.Parse("7a1c0000-0000-4000-8000-0000000000e1");
    private static readonly Guid CreationEvent = Guid.Parse("7a1c0000-0000-4000-8000-0000000000e2");
    private static readonly Guid DeletionEvent = Guid.Parse("7a1c0000-0000-4000-8000-0000000000e3");

    private readonly PostgresFixture postgres = new();

    public Task InitializeAsync() => postgres.StartWithoutMigratingAsync();

    public Task DisposeAsync() => postgres.DisposeAsync();

    [Fact]
    public async Task Migration_ShouldMergeThePayloadTablesIntoItemEventsAndBack()
    {
        await using var context = postgres.CreateContext();
        var migrator = context.GetService<IMigrator>();

        await migrator.MigrateAsync(BeforeMigration);

        // The interpolated values are this test's own GUID constants.
#pragma warning disable EF1002
        await context.Database.ExecuteSqlRawAsync(
            $"""
            INSERT INTO entities (guid, entity_type)
            VALUES ('{WidgetArticle}', 0), ('{BoxArticle}', 0), ('{SandArticle}', 0),
                   ('{BoxOne}', 1), ('{BoxTwo}', 1), ('{Widget}', 1);

            INSERT INTO articles (guid, name, description, article_type, tracking_mode)
            VALUES ('{WidgetArticle}', 'Widget', '', 1, 1),
                   ('{BoxArticle}', 'Box', '', 5, 1),
                   ('{SandArticle}', 'Sand', '', 1, 2);

            INSERT INTO items (guid, article_guid, parent_item_container_guid, is_fixed)
            VALUES ('{BoxOne}', '{BoxArticle}', NULL, false),
                   ('{BoxTwo}', '{BoxArticle}', NULL, false),
                   ('{Widget}', '{WidgetArticle}', '{BoxTwo}', false);

            INSERT INTO events (guid, event_type, occured_at)
            VALUES ('{ReceiptEvent}', 0, '2026-01-01T10:00:00Z'),
                   ('{CreationEvent}', 0, '2026-01-02T10:00:00Z'),
                   ('{DeletionEvent}', 0, '2026-01-03T10:00:00Z');

            INSERT INTO item_events (guid, article_guid, item_event_type)
            VALUES ('{ReceiptEvent}', '{SandArticle}', 1),
                   ('{CreationEvent}', '{SandArticle}', 2),
                   ('{DeletionEvent}', '{SandArticle}', 3);

            INSERT INTO item_moviment_events
                (item_event_guid, quantity, from_item_container_guid, from_no_container, to_item_container_guid, to_no_container)
            VALUES ('{ReceiptEvent}', 50, NULL, true, '{BoxOne}', false);

            INSERT INTO item_lifecycle_events (item_event_guid, quantity, item_container_guid, no_container)
            VALUES ('{CreationEvent}', 120, '{BoxOne}', false),
                   ('{DeletionEvent}', 7, NULL, true);

            INSERT INTO item_moviments (item_guid, occurred_at, moved_to_container_guid, removed_from_containers)
            VALUES ('{Widget}', '2026-01-04T10:00:00Z', '{BoxOne}', false),
                   ('{Widget}', '2026-01-05T10:00:00Z', '{BoxTwo}', false),
                   ('{Widget}', '2026-01-06T10:00:00Z', NULL, true);
            """);
#pragma warning restore EF1002

        await migrator.MigrateAsync();

        var receipt = await EventAsync(context, ReceiptEvent);
        Assert.Equal((1, 50d, null, true, BoxOne, false), receipt);

        // A creation has no origin and a deletion has no destination.
        Assert.Equal((2, 120d, null, true, BoxOne, false), await EventAsync(context, CreationEvent));
        Assert.Equal((3, 7d, null, true, null, true), await EventAsync(context, DeletionEvent));

        // The origin of each movement of the item is where the previous one sent it.
        var itemMoves = await context.Database
            .SqlQuery<MoveRow>(
                $"""
                SELECT e.from_item_container_guid AS from_guid, e.from_no_container AS from_none,
                       e.to_item_container_guid AS to_guid, e.to_no_container AS to_none
                FROM item_events AS e
                INNER JOIN events AS ev ON ev.guid = e.guid
                WHERE e.item_guid = {Widget}
                ORDER BY ev.occured_at
                """)
            .ToListAsync();

        Assert.Equal(
            [
                new MoveRow(null, true, BoxOne, false),
                new MoveRow(BoxOne, false, BoxTwo, false),
                new MoveRow(BoxTwo, false, null, true),
            ],
            itemMoves);

        await migrator.MigrateAsync(BeforeMigration);

        Assert.Equal(
            1, await ScalarAsync(context, $"SELECT count(*)::integer AS \"Value\" FROM item_moviment_events WHERE item_event_guid = '{ReceiptEvent}' AND quantity = 50 AND to_item_container_guid = '{BoxOne}'"));
        Assert.Equal(
            1, await ScalarAsync(context, $"SELECT count(*)::integer AS \"Value\" FROM item_lifecycle_events WHERE item_event_guid = '{CreationEvent}' AND quantity = 120 AND item_container_guid = '{BoxOne}'"));
        Assert.Equal(
            1, await ScalarAsync(context, $"SELECT count(*)::integer AS \"Value\" FROM item_lifecycle_events WHERE item_event_guid = '{DeletionEvent}' AND no_container"));
        Assert.Equal(
            3, await ScalarAsync(context, $"SELECT count(*)::integer AS \"Value\" FROM item_moviments WHERE item_guid = '{Widget}'"));
        Assert.Equal(
            3, await ScalarAsync(context, "SELECT count(*)::integer AS \"Value\" FROM item_events"));
    }

    private static async Task<(int Type, double Quantity, Guid? From, bool FromNone, Guid? To, bool ToNone)> EventAsync(
        DbContext context, Guid guid)
    {
        var row = await context.Database
            .SqlQuery<EventRow>(
                $"""
                SELECT item_event_type::integer AS type, quantity AS quantity,
                       from_item_container_guid AS from_guid, from_no_container AS from_none,
                       to_item_container_guid AS to_guid, to_no_container AS to_none
                FROM item_events
                WHERE guid = {guid}
                """)
            .SingleAsync();

        return (row.Type, row.Quantity, row.FromGuid, row.FromNone, row.ToGuid, row.ToNone);
    }

    private static async Task<int> ScalarAsync(DbContext context, string sql)
#pragma warning disable EF1002
        => await context.Database.SqlQueryRaw<int>(sql).SingleAsync();
#pragma warning restore EF1002

    private sealed record EventRow(int Type, double Quantity, Guid? FromGuid, bool FromNone, Guid? ToGuid, bool ToNone);

    private sealed record MoveRow(Guid? FromGuid, bool FromNone, Guid? ToGuid, bool ToNone);
}
