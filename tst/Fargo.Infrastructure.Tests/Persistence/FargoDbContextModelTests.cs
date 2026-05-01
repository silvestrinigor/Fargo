using Fargo.Domain.Articles;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Persistence;

public sealed class FargoDbContextModelTests
{
    [Fact]
    public void Model_Should_MapBarcodesToArticlesTable()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FargoModelTests;Trusted_Connection=True;")
            .Options;

        using var context = new FargoDbContext(options);

        var barcodesType = context.Model.FindEntityType(typeof(ArticleBarcodes));

        Assert.NotNull(barcodesType);
        Assert.Equal("Articles", barcodesType.GetTableName());
        Assert.NotNull(barcodesType.FindProperty(nameof(ArticleBarcodes.Ean13)));
    }
}
