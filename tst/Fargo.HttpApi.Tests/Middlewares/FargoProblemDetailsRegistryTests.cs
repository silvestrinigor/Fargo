using Fargo.Application;
using Fargo.Core.Articles;

namespace Fargo.HttpApi.Tests.Middlewares;

public sealed class FargoProblemDetailsRegistryTests
{

    [Fact]
    public void Registry_Should_Map_BarcodeConflict_To_Conflict()
    {
        var definition = FargoProblemDetailsRegistry.Map[typeof(ArticleBarcodeAlreadyInUseFargoDomainException)];

        Assert.Equal(409, definition.StatusCode);
        Assert.Equal("barcode/already-exists", definition.Type);
    }

    [Fact]
    public void Registry_Should_Map_EntityAccessViolation_To_Forbidden()
    {
        var definition = FargoProblemDetailsRegistry.Map[typeof(EntityAccessViolationFargoApplicationException)];

        Assert.Equal(403, definition.StatusCode);
        Assert.Equal("entity/access-denied", definition.Type);
    }

    [Fact]
    public void Registry_Should_Map_GeneralArgumentExceptions_To_InvalidRequest()
    {
        Assert.Equal("request/invalid", FargoProblemDetailsRegistry.Map[typeof(ArgumentException)].Type);
        Assert.Equal("request/invalid", FargoProblemDetailsRegistry.Map[typeof(ArgumentNullException)].Type);
        Assert.Equal("request/invalid", FargoProblemDetailsRegistry.Map[typeof(ArgumentOutOfRangeException)].Type);
    }
}
