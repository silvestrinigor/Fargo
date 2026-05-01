using Fargo.Api.Articles;

namespace Fargo.Api.Tests.Articles;

public sealed class ArticleMetricsTests
{
    // --- Density computation ---

    [Fact]
    public void Density_Should_BeNull_When_MassIsNull()
    {
        var metrics = new ArticleMetrics
        {
            LengthX = new Length(1, LengthUnit.Meter),
            LengthY = new Length(1, LengthUnit.Meter),
            LengthZ = new Length(1, LengthUnit.Meter),
        };

        Assert.Null(metrics.Density);
    }

    [Fact]
    public void Density_Should_BeNull_When_AnyLengthIsNull()
    {
        var metrics = new ArticleMetrics
        {
            Mass = new Mass(1, MassUnit.Kilogram),
            LengthX = new Length(1, LengthUnit.Meter),
            LengthY = new Length(1, LengthUnit.Meter),
        };

        Assert.Null(metrics.Density);
    }

    [Fact]
    public void Density_Should_BeNull_When_LengthIsZero()
    {
        var metrics = new ArticleMetrics
        {
            Mass = new Mass(1, MassUnit.Kilogram),
            LengthX = new Length(0, LengthUnit.Meter),
            LengthY = new Length(1, LengthUnit.Meter),
            LengthZ = new Length(1, LengthUnit.Meter),
        };

        Assert.Null(metrics.Density);
    }

    [Fact]
    public void Density_Should_UseKgPerM3_When_MassIsKgAndLengthsAreMeters()
    {
        var metrics = new ArticleMetrics
        {
            Mass = new Mass(1, MassUnit.Kilogram),
            LengthX = new Length(1, LengthUnit.Meter),
            LengthY = new Length(1, LengthUnit.Meter),
            LengthZ = new Length(1, LengthUnit.Meter),
        };

        Assert.NotNull(metrics.Density);
        Assert.Equal(1.0, metrics.Density.Value, precision: 10);
        Assert.Equal(DensityUnit.KilogramPerCubicMeter, metrics.Density.Unit);
    }

    [Fact]
    public void Density_Should_UseGPerCm3_When_MassIsGramsAndLengthsAreCentimeters()
    {
        var metrics = new ArticleMetrics
        {
            Mass = new Mass(1, MassUnit.Gram),
            LengthX = new Length(1, LengthUnit.Centimeter),
            LengthY = new Length(1, LengthUnit.Centimeter),
            LengthZ = new Length(1, LengthUnit.Centimeter),
        };

        Assert.NotNull(metrics.Density);
        Assert.Equal(1.0, metrics.Density.Value, precision: 10);
        Assert.Equal(DensityUnit.GramPerCubicCentimeter, metrics.Density.Unit);
    }

    [Fact]
    public void Density_Should_CorrectlyConvert_When_MixedUnits()
    {
        // 1000 g in a 1m × 1m × 1m cube → 1000 g/m³ = 1 kg/m³
        // Since mass is grams and length is meters, natural unit is g/m³
        var metrics = new ArticleMetrics
        {
            Mass = new Mass(1000, MassUnit.Gram),
            LengthX = new Length(1, LengthUnit.Meter),
            LengthY = new Length(1, LengthUnit.Meter),
            LengthZ = new Length(1, LengthUnit.Meter),
        };

        Assert.NotNull(metrics.Density);
        Assert.Equal(1000.0, metrics.Density.Value, precision: 10);
        Assert.Equal(DensityUnit.GramPerCubicMeter, metrics.Density.Unit);
    }

    [Fact]
    public void Density_Should_FallbackToKgPerM3_When_NoNaturalUnit()
    {
        // oz mass + m lengths — no natural density unit defined, falls back to kg/m³
        var metrics = new ArticleMetrics
        {
            Mass = new Mass(1, MassUnit.Ounce),
            LengthX = new Length(1, LengthUnit.Meter),
            LengthY = new Length(1, LengthUnit.Meter),
            LengthZ = new Length(1, LengthUnit.Meter),
        };

        Assert.NotNull(metrics.Density);
        Assert.Equal(DensityUnit.KilogramPerCubicMeter, metrics.Density.Unit);
    }
}
