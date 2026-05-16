using Fargo.Core.Barcodes;
using UnitsNet;

namespace Fargo.Core.Articles;

#region Metrics

/// <summary>
/// Defines the physical metrics of an article.
/// </summary>
public readonly struct ArticleMetrics : IEquatable<ArticleMetrics>
{
    public ArticleMetrics(
        Mass? mass = null,
        Length? lengthX = null,
        Length? lengthY = null,
        Length? lengthZ = null)
    {
        ValidatePositive(mass, nameof(mass));
        ValidatePositive(lengthX, nameof(lengthX));
        ValidatePositive(lengthY, nameof(lengthY));
        ValidatePositive(lengthZ, nameof(lengthZ));

        Mass = mass;
        LengthX = lengthX;
        LengthY = lengthY;
        LengthZ = lengthZ;
    }

    public Mass? Mass { get; }

    public Length? LengthX { get; }

    public Length? LengthY { get; }

    public Length? LengthZ { get; }

    public bool Equals(ArticleMetrics other)
        => Nullable.Equals(Mass, other.Mass) &&
           Nullable.Equals(LengthX, other.LengthX) &&
           Nullable.Equals(LengthY, other.LengthY) &&
           Nullable.Equals(LengthZ, other.LengthZ);

    public override bool Equals(object? obj) => obj is ArticleMetrics other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Mass, LengthX, LengthY, LengthZ);

    public static bool operator ==(ArticleMetrics left, ArticleMetrics right) => left.Equals(right);

    public static bool operator !=(ArticleMetrics left, ArticleMetrics right) => !left.Equals(right);

    private static void ValidatePositive(Mass? value, string paramName)
    {
        if (value is not null && value.Value <= UnitsNet.Mass.Zero)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                "Article mass must be greater than zero.");
        }
    }

    private static void ValidatePositive(Length? value, string paramName)
    {
        if (value is not null && value.Value <= UnitsNet.Length.Zero)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                "Article length must be greater than zero.");
        }
    }
}

#endregion Metrics

#region Barcodes

/// <summary>
/// Defines the barcode set assigned to an article.
/// </summary>
public readonly record struct ArticleBarcodesSet(
    Ean13? Ean13 = null,
    Ean8? Ean8 = null,
    UpcA? UpcA = null,
    UpcE? UpcE = null,
    Code128? Code128 = null,
    Code39? Code39 = null,
    Itf14? Itf14 = null,
    Gs1128? Gs1128 = null,
    QrCode? QrCode = null,
    DataMatrix? DataMatrix = null
);

#endregion Barcodes
