using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application;

#region Pagination

/// <summary>
/// Represents the page index used in paginated queries.
/// </summary>
/// <remarks>
/// This value object ensures that page numbers remain within
/// a valid range and supports automatic parsing from strings
/// using <see cref="IParsable{TSelf}"/>.
/// </remarks>
public readonly struct Page
    : IParsable<Page>
{
    /// <summary>
    /// Gets the first valid page (<c>1</c>).
    /// </summary>
    /// <remarks>
    /// Provides a safe starting page for pagination operations.
    /// </remarks>
    public static Page FirstPage => new(1);

    /// <summary>
    /// Initializes a new instance of <see cref="Page"/>.
    /// </summary>
    /// <param name="value">The page number.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the page value is outside the allowed range.
    /// </exception>
    public Page(int value)
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
    /// Minimum allowed page value.
    /// </summary>
    public const int MinValue = 1;

    /// <summary>
    /// Maximum allowed page value.
    /// </summary>
    public const int MaxValue = int.MaxValue;

    private readonly int value;

    /// <summary>
    /// Gets the page value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the instance was not properly initialized.
    /// </exception>
    public int Value => value == 0
        ? throw new InvalidOperationException(
            $"{nameof(Page)} was not initialized. Do not use the default value of this struct.")
        : value;

    /// <summary>
    /// Implicitly converts <see cref="Page"/> to <see cref="int"/>.
    /// </summary>
    public static implicit operator int(Page page)
        => page.Value;

    /// <summary>
    /// Explicitly converts an <see cref="int"/> to <see cref="Page"/>.
    /// </summary>
    public static explicit operator Page(int value)
        => new(value);

    /// <summary>
    /// Parses a string into a <see cref="Page"/>.
    /// </summary>
    public static Page Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid Page value: '{s}'.");
    }

    /// <summary>
    /// Attempts to parse a string into a <see cref="Page"/>.
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Page result)
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

        result = new Page(value);
        return true;
    }
}
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

/// <summary>
/// Represents pagination parameters used in paginated queries.
/// </summary>
/// <remarks>
/// Pagination combines a <see cref="Page"/> index and a <see cref="Limit"/>
/// to determine which subset of records should be retrieved.
///
/// The <see cref="Skip"/> and <see cref="Take"/> values can be used directly
/// in data queries such as database pagination.
/// </remarks>
public readonly record struct Pagination
{
    /// <summary>
    /// Initializes a new instance of <see cref="Pagination"/>.
    /// </summary>
    /// <param name="page">
    /// The page index that determines which page of results to retrieve.
    /// </param>
    /// <param name="limit">
    /// The maximum number of items to return per page.
    /// </param>
    /// <remarks>
    /// Both <paramref name="page"/> and <paramref name="limit"/> must be
    /// valid initialized values. Using the default struct value for either
    /// may result in an <see cref="InvalidOperationException"/> when accessed.
    /// </remarks>
    public Pagination(
        Page page,
        Limit limit)
    {
        Page = page;
        Limit = limit;
    }

    /// <summary>
    /// Gets the page index used for pagination.
    /// </summary>
    public Page Page { get; init; }

    /// <summary>
    /// Gets the maximum number of items returned per page.
    /// </summary>
    public Limit Limit { get; init; }

    /// <summary>
    /// Gets the number of records to skip when executing a paginated query.
    /// </summary>
    /// <remarks>
    /// Calculated as: (<see cref="Page"/> - 1) × <see cref="Limit"/>.
    /// </remarks>
    public int Skip => (Page.Value - 1) * Limit.Value;

    /// <summary>
    /// Gets the maximum number of records to retrieve.
    /// </summary>
    /// <remarks>
    /// This value is equivalent to <see cref="Limit"/>.
    /// </remarks>
    public int Take => Limit.Value;
}

#endregion Pagination

#region Partition Query Filters

public static class PartitionQueryFilter
{
    public static (
        IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions,
        bool? NotChildOfAnyPartition) ForPartitionedEntities(
            IReadOnlyCollection<Guid> actorPartitionGuids,
            IReadOnlyCollection<Guid>? requestedPartitionGuids,
            bool? notChildOfAnyPartition)
    {
        if (requestedPartitionGuids is { Count: > 0 })
        {
            return (
                [.. actorPartitionGuids.Intersect(requestedPartitionGuids)],
                notChildOfAnyPartition);
        }

        if (notChildOfAnyPartition is true)
        {
            return (null, true);
        }

        return (actorPartitionGuids, notChildOfAnyPartition ?? true);
    }
}

#endregion Partition Query Filters
