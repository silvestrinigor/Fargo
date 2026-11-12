using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// Used only by the <c>dotnet ef</c> tooling (for example to add a migration), never at runtime.
/// Schema upgrades are applied by the migration service.
/// </summary>
public sealed class FargoDesignTimeDbContextFactory : IDesignTimeDbContextFactory<FargoDbContext>
{
    public FargoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FargoDbContext>();

        optionsBuilder
        .UseNpgsql(
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
            })
        .UseSnakeCaseNamingConvention();

        return new FargoDbContext(optionsBuilder.Options);
    }
}
