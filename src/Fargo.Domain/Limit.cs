using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Domain.ValueObjects;

/// <summary>
/// Represents the maximum number of items returned in a paginated query.
/// </summary>
/// <remarks>
/// This value object ensures that pagination limits stay within
/// a safe and controlled range.
///
/// It implements <see cref="IParsable{TSelf}"/> so it can be automatically
/// parsed from query parameters in ASP.NET.
/// </remarks>
public readonly struct Limit
    : IParsable<Limit>
{
    /// <summary>
    /// Gets the maximum valid limit.
    /// </summary>
    /// <remarks>
    /// This property represents the largest number of items
    /// that can be requested in a paginated query.
    ///
    /// It is equivalent to creating a new instance with
    /// <see cref="MaxValue"/>.
    /// </remarks>
    public static Limit MaxLimit => new(MaxValue);

    /// <summary>
    /// Initializes a new instance of <see cref="Limit"/>.
    /// </summary>
    /// <param name="value">The limit value.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is outside the allowed range.
    /// </exception>
    public Limit(int value)
    {
        if (value < MinValue || value > MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Must be between {MinValue} and {MaxValue}.");
        }

        this.value = value;
    }

    /// <summary>
    /// Minimum allowed limit value.
    /// </summary>
    public const int MinValue = 1;

    /// <summary>
    /// Maximum allowed limit value.
    /// </summary>
    public const int MaxValue = 1000;

    private readonly int value;

    /// <summary>
    /// Gets the limit value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the instance was not properly initialized.
    /// </exception>
    public int Value => value == 0
        ? throw new InvalidOperationException(
            $"{nameof(Limit)} was not initialized. Do not use the default value of this struct.")
        : value;

    /// <summary>
    /// Implicitly converts <see cref="Limit"/> to <see cref="int"/>.
    /// </summary>
    public static implicit operator int(Limit limit)
        => limit.Value;

    /// <summary>
    /// Explicitly converts an <see cref="int"/> to <see cref="Limit"/>.
    /// </summary>
    public static explicit operator Limit(int value)
        => new(value);

    /// <summary>
    /// Parses a string into a <see cref="Limit"/>.
    /// </summary>
    public static Limit Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid Limit value: '{s}'.");
    }

    /// <summary>
    /// Attempts to parse a string into a <see cref="Limit"/>.
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Limit result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var parsed = int.TryParse(
            s,
            NumberStyles.Integer,
            provider ?? CultureInfo.InvariantCulture,
            out var value);

        if (!parsed)
        {
            return false;
        }

        if (value < MinValue || value > MaxValue)
        {
            return false;
        }

        result = new Limit(value);
        return true;
    }
}
