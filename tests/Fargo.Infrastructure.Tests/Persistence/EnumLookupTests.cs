using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Events;
using Fargo.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fargo.Infrastructure.Tests.Persistence;

/// <summary>
/// The reference tables are filled by the migrations, so they drift from the enums if a member is added
/// without a new migration.
/// </summary>
public class EnumLookupTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    public static TheoryData<string, Type> Lookups => new()
    {
        { "entity_types", typeof(EntityType) },
        { "event_types", typeof(EventType) },
        { "item_event_types", typeof(ItemEventType) },
        { "actor_types", typeof(ActorType) },
        { "article_types", typeof(ArticleType) },
        { "article_tracking_modes", typeof(ArticleTrackingMode) },
        { "supply_order_statuses", typeof(SupplyOrderStatus) },
        { "audit_log_types", typeof(AuditLogType) },
        { "action_types", typeof(ActionType) },
    };

    [Theory]
    [MemberData(nameof(Lookups))]
    public async Task LookupTable_ShouldHoldExactlyTheEnumMembers(string table, Type enumType)
    {
        var expected = Enum.GetValues(enumType)
            .Cast<Enum>()
            .Select(value => $"{Convert.ToInt64(value)}:{value}")
            .Order(StringComparer.Ordinal)
            .ToList();

        await using var context = postgres.CreateContext();

        var actual = await context.Database
            .SqlQuery<string>($"SELECT id::text || ':' || name AS \"Value\" FROM {table}")
            .ToListAsync();

        Assert.Equal(expected, actual.Order(StringComparer.Ordinal).ToList());
    }

    [Fact]
    public async Task UnknownEnumValue_ShouldBeRejectedByTheForeignKey()
    {
        await using var context = postgres.CreateContext();

        var exception = await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlRawAsync(
            "INSERT INTO entities (guid, entity_type) VALUES (gen_random_uuid(), 9999)"));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception.SqlState);
    }
}
