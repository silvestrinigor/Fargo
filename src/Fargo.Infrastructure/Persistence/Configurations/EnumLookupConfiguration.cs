using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Events;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the reference table of every enum that is stored in the database and the foreign keys that point at them.
/// </summary>
internal static class EnumLookupConfiguration
{
    public const int NameMaxLength = 100;

    public static ModelBuilder ApplyEnumLookups(this ModelBuilder modelBuilder)
    {
        modelBuilder
        .HasEnumLookup<EntityType>("entity_types")
        .HasEnumLookup<EventType>("event_types")
        .HasEnumLookup<ItemEventType>("item_event_types")
        .HasEnumLookup<ActorType>("actor_types")
        .HasEnumLookup<ArticleType>("article_types")
        .HasEnumLookup<ArticleTrackingMode>("article_tracking_modes")
        .HasEnumLookup<SupplyOrderStatus>("supply_order_statuses")
        .HasEnumLookup<AuditLogType>("audit_log_types")
        .HasEnumLookup<ActionType>("action_types");

        modelBuilder.Entity<Entity>().HasEnumForeignKey(x => x.EntityType);
        modelBuilder.Entity<Event>().HasEnumForeignKey(x => x.EventType);
        modelBuilder.Entity<ItemEvent>().HasEnumForeignKey(x => x.ItemEventType);
        modelBuilder.Entity<Actor>().HasEnumForeignKey(x => x.ActorType);
        modelBuilder.Entity<AuditLog>().HasEnumForeignKey(x => x.ActorType);
        modelBuilder.Entity<AuditLog>().HasEnumForeignKey(x => x.AuditLogType);
        modelBuilder.Entity<Article>().HasEnumForeignKey(x => x.ArticleType);
        modelBuilder.Entity<Article>().HasEnumForeignKey(x => x.TrackingMode);
        modelBuilder.Entity<SupplyOrder>().HasEnumForeignKey(x => x.Status);
        modelBuilder.Entity<ActorPermission>().HasEnumForeignKey(x => x.ActionType);
        modelBuilder.Entity<UserGroupPermission>().HasEnumForeignKey(x => x.ActionType);

        return modelBuilder;
    }

    private static ModelBuilder HasEnumLookup<TEnum>(this ModelBuilder modelBuilder, string tableName)
        where TEnum : struct, Enum
    {
        modelBuilder.Entity<EnumLookup<TEnum>>(builder =>
        {
            builder.ToTable(tableName);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Name).HasMaxLength(NameMaxLength).IsRequired();

            builder.HasIndex(x => x.Name).IsUnique();

            builder.HasData(Enum.GetValues<TEnum>().Select(value => new EnumLookup<TEnum>
            {
                Id = value,
                Name = value.ToString(),
            }));
        });

        return modelBuilder;
    }
}
