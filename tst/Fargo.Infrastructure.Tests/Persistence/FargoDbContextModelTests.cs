using Fargo.Domain.Barcodes;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Persistence;

public sealed class FargoDbContextModelTests
{
    [Fact]
    public void Model_Should_MapEan13ToArticleEan13Table()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FargoModelTests;Trusted_Connection=True;")
            .Options;

        using var context = new FargoDbContext(options);

        var ean13Type = context.Model.FindEntityType(typeof(Ean13Data));

        Assert.NotNull(ean13Type);
        Assert.Equal("ArticleEan13", ean13Type.GetTableName());
    }
}
