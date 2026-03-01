using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fargo.Infrastructure.Persistence
{
    public sealed class FargoWriteDesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<FargoWriteDbContext>
    {
        public FargoWriteDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FargoWriteDbContext>();

            var cs = Environment
                .GetEnvironmentVariable("FARGO_CONNECTION_STRING")
                ?? "Host=localhost;Database=fargo;Username=postgres;Password=postgres";

            optionsBuilder.UseSqlServer(cs);

            return new FargoWriteDbContext(optionsBuilder.Options);
        }
    }
}