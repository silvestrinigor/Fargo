using Fargo.Api.Middlewares;
using Fargo.Application;
using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Api.Tests.Middlewares;

public sealed class FargoProblemDetailsRegistryTests
{
    [Fact]
    public void Registry_Should_Map_All_Concrete_Fargo_Application_And_Domain_Exceptions()
    {
        var exceptionTypes = typeof(FargoApplicationException).Assembly
            .GetTypes()
            .Concat(typeof(FargoDomainException).Assembly.GetTypes())
            .Where(static type =>
                type is { IsAbstract: false } &&
                type != typeof(FargoApplicationException) &&
                typeof(Exception).IsAssignableFrom(type) &&
                (typeof(FargoApplicationException).IsAssignableFrom(type) ||
                 typeof(FargoDomainException).IsAssignableFrom(type)))
            .Distinct()
            .OrderBy(static type => type.FullName)
            .ToArray();

        var missing = exceptionTypes
            .Where(static type => !FargoProblemDetailsRegistry.Map.ContainsKey(type))
            .Select(static type => type.FullName)
            .ToArray();

        Assert.Empty(missing);
    }

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
