using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fargo.Infrastructure.Persistence
{
    /// <summary>
    /// Design-time factory used by Entity Framework Core tools to create
    /// an instance of <see cref="FargoWriteDbContext"/>.
    /// </summary>
    /// <remarks>
    /// This factory exists exclusively for <b>design-time operations</b> executed
    /// by the <c>dotnet ef</c> tooling, such as generating new migrations.
    ///
    /// The application itself does not use this factory at runtime.
    ///
    /// Database schema upgrades are performed by a dedicated worker service
    /// responsible for applying migrations during deployment or startup.
    /// </remarks>
    public sealed class FargoWriteDesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<FargoWriteDbContext>
    {
        /// <summary>
        /// Creates a configured instance of <see cref="FargoWriteDbContext"/>
        /// used by Entity Framework Core design-time tools.
        /// </summary>
        /// <param name="args">
        /// Arguments provided by the EF Core tooling.
        /// </param>
        /// <returns>
        /// A <see cref="FargoWriteDbContext"/> instance configured for
        /// design-time operations such as migration generation.
        /// </returns>
        /// <remarks>
        /// This method is invoked by the <c>dotnet ef</c> CLI when executing
        /// commands like <c>dotnet ef migrations add</c>.
        ///
        /// It is not used for applying migrations or running the application.
        /// Database updates are handled by a separate worker service.
        /// </remarks>
        public FargoWriteDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FargoWriteDbContext>();

            optionsBuilder.UseSqlServer();

            return new FargoWriteDbContext(optionsBuilder.Options);
        }
    }
}