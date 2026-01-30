using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Fargo.Infrastructure.Persistence.Write
{
    public class FargoWriteDbContextFactory : IDesignTimeDbContextFactory<FargoWriteDbContext>
    {
        public FargoWriteDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("Fargo");

            var optionsBuilder = new DbContextOptionsBuilder<FargoWriteDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FargoWriteDbContext(optionsBuilder.Options);
        }
    }
}