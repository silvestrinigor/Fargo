using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Common;

/// <summary>
/// Creates isolated in-memory <see cref="FargoDbContext"/> instances for
/// repository tests.
/// </summary>
/// <remarks>
/// Each call uses a unique database name so tests are fully isolated. The
/// EF Core in-memory provider is a LINQ in-process query engine: it supports
/// LINQ-translatable queries but NOT raw SQL (<c>SqlQuery</c>/<c>FromSql</c>),
/// recursive CTEs, or PostgreSQL range operators. Repository methods that rely
/// on those (descendant walks, temporal inventory/location, permission/partition
/// trees) are therefore covered by the PostgreSQL integration checks elsewhere,
/// not here.
/// </remarks>
public static class TestDbContextFactory
{
    public static FargoDbContext Create()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FargoDbContext(options);
    }
}
