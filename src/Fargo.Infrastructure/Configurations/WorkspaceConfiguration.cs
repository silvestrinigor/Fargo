using Fargo.Core.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fargo.Infrastructure.Configurations;

public sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces");

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.ActorGuid).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CommittedAt);
        builder.Property(x => x.RolledBackAt);

        builder
            .HasMany(x => x.Commands)
            .WithOne(x => x.Workspace)
            .HasForeignKey(x => x.WorkspaceGuid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Commands)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.ActorGuid);
        builder.HasIndex(x => x.Status);
    }
}

public sealed class WorkspaceCommandConfiguration : IEntityTypeConfiguration<WorkspaceCommand>
{
    public void Configure(EntityTypeBuilder<WorkspaceCommand> builder)
    {
        builder.ToTable("WorkspaceCommands");

        builder.HasKey(x => x.Guid);

        builder.Property(x => x.WorkspaceGuid).IsRequired();
        builder.Property(x => x.CommandId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.CommandType).IsRequired().HasMaxLength(128);
        builder.Property(x => x.CommandVersion).IsRequired();
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.ReservedEntityGuid);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExecutedAt);

        builder.HasIndex(x => new { x.WorkspaceGuid, x.Sequence }).IsUnique();
        builder.HasIndex(x => new { x.WorkspaceGuid, x.CommandId }).IsUnique();
        builder.HasIndex(x => x.CommandType);
        builder.HasIndex(x => x.ReservedEntityGuid);
    }
}
