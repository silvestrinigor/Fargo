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
/// not here. The same applies to ordered list paging over the Table-Per-Type
/// (TPT) entity hierarchy: the InMemory provider cannot shape the base/derived
/// join under an <c>OrderBy</c>, so those repository list tests are skipped here
/// and covered by the PostgreSQL checks instead.
/// </remarks>
public static class TestDbContextFactory
{
    public static FargoDbContext Create()
        => Create(Guid.NewGuid().ToString());

    public static FargoDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new FargoDbContext(options);
    }
}
