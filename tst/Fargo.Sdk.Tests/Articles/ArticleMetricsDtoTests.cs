using Fargo.Sdk.Articles;

namespace Fargo.Sdk.Tests.Articles;

public sealed class ArticleMetricsDtoTests
{
    // --- Density computation ---

    [Fact]
    public void Density_Should_BeNull_When_MassIsNull()
    {
        var dto = new ArticleMetricsDto
        {
            LengthX = new LengthDto(1, "m"),
            LengthY = new LengthDto(1, "m"),
            LengthZ = new LengthDto(1, "m"),
        };

        Assert.Null(dto.Density);
    }

    [Fact]
    public void Density_Should_BeNull_When_AnyLengthIsNull()
    {
        var dto = new ArticleMetricsDto
        {
            Mass = new MassDto(1, "kg"),
            LengthX = new LengthDto(1, "m"),
            LengthY = new LengthDto(1, "m"),
        };

        Assert.Null(dto.Density);
    }

    [Fact]
    public void Density_Should_BeNull_When_LengthIsZero()
    {
        var dto = new ArticleMetricsDto
        {
            Mass = new MassDto(1, "kg"),
            LengthX = new LengthDto(0, "m"),
            LengthY = new LengthDto(1, "m"),
            LengthZ = new LengthDto(1, "m"),
        };

        Assert.Null(dto.Density);
    }

    [Fact]
    public void Density_Should_UseKgPerM3_When_MassIsKgAndLengthsAreMeters()
    {
        var dto = new ArticleMetricsDto
        {
            Mass = new MassDto(1, "kg"),
            LengthX = new LengthDto(1, "m"),
            LengthY = new LengthDto(1, "m"),
            LengthZ = new LengthDto(1, "m"),
        };

        Assert.NotNull(dto.Density);
        Assert.Equal(1.0, dto.Density.Value, precision: 10);
        Assert.Equal("kg/m³", dto.Density.Unit);
    }

    [Fact]
    public void Density_Should_UseGPerCm3_When_MassIsGramsAndLengthsAreCentimeters()
    {
        var dto = new ArticleMetricsDto
        {
            Mass = new MassDto(1, "g"),
            LengthX = new LengthDto(1, "cm"),
            LengthY = new LengthDto(1, "cm"),
            LengthZ = new LengthDto(1, "cm"),
        };

        Assert.NotNull(dto.Density);
        Assert.Equal(1.0, dto.Density.Value, precision: 10);
        Assert.Equal("g/cm³", dto.Density.Unit);
    }

    [Fact]
    public void Density_Should_CorrectlyConvert_When_MixedUnits()
    {
        // 1000 g in a 1m × 1m × 1m cube → 1000 g/m³ = 1 kg/m³
        // Since mass is grams and length is meters, natural unit is g/m³
        var dto = new ArticleMetricsDto
        {
            Mass = new MassDto(1000, "g"),
            LengthX = new LengthDto(1, "m"),
            LengthY = new LengthDto(1, "m"),
            LengthZ = new LengthDto(1, "m"),
        };

        Assert.NotNull(dto.Density);
        Assert.Equal(1000.0, dto.Density.Value, precision: 10);
        Assert.Equal("g/m³", dto.Density.Unit);
    }

    [Fact]
    public void Density_Should_FallbackToKgPerM3_When_NoNaturalUnit()
    {
        // oz mass + m lengths — no natural density unit defined, falls back to kg/m³
        var dto = new ArticleMetricsDto
        {
            Mass = new MassDto(1, "oz"),
            LengthX = new LengthDto(1, "m"),
            LengthY = new LengthDto(1, "m"),
            LengthZ = new LengthDto(1, "m"),
        };

        Assert.NotNull(dto.Density);
        Assert.Equal("kg/m³", dto.Density.Unit);
    }
}
