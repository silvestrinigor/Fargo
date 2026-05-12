using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application;

#region Commands

/// <summary>
/// Represents a command in the application layer.
///
/// Commands represent operations that change the state of the system
/// and do not return a result.
/// </summary>
public interface ICommand;

/// <summary>
/// Represents a command that returns a response.
/// </summary>
/// <typeparam name="TResponse">
/// The type of response returned by the command.
/// </typeparam>
public interface ICommand<out TResponse>;

/// <summary>
/// Defines a handler responsible for executing a command.
/// </summary>
/// <typeparam name="TCommand">
/// The type of command handled by this handler.
/// </typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(
        TCommand command,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Defines a handler responsible for executing a command that produces a response.
/// </summary>
/// <typeparam name="TCommand">
/// The type of command handled by this handler.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of response returned by the command.
/// </typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The response produced by the command.</returns>
    Task<TResponse> Handle(
        TCommand command,
        CancellationToken cancellationToken = default
    );
}

#endregion Commands

#region Queries

/// <summary>
/// Represents a query in the application layer.
///
/// Queries are used to retrieve data and must not change the state
/// of the system.
/// </summary>
/// <typeparam name="TResponse">
/// The type of response returned by the query.
/// </typeparam>
public interface IQuery<out TResponse>;

/// <summary>
/// Defines a handler responsible for executing a query.
/// </summary>
/// <typeparam name="TQuery">
/// The type of query handled by this handler.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of response returned by the query.
/// </typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Executes the specified query.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// The result produced by the query.
    /// </returns>
    Task<TResponse> Handle(
        TQuery query,
        CancellationToken cancellationToken = default
    );
}

#endregion Queries

#region Unit Of Work

/// <summary>
/// Represents a unit of work responsible for committing changes
/// made during the execution of an application operation.
/// </summary>
/// <remarks>
/// The unit of work coordinates the persistence of changes across
/// repositories and ensures that they are committed as a single
/// transaction.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all changes made within the current unit of work.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The number of state entries written to the database.
    /// </returns>
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
}

#endregion Unit Of Work

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

#region Exceptions

/// <summary>
/// Represents the base exception type for errors that occur
/// in the application layer.
/// </summary>
/// <remarks>
/// Application exceptions represent failures related to
/// application use cases, orchestration, or external inputs,
/// but not domain rule violations.
/// </remarks>
public class FargoApplicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="FargoApplicationException"/>.
    /// </summary>
    public FargoApplicationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FargoApplicationException"/>
    /// with a specified error message.
    /// </summary>
    public FargoApplicationException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FargoApplicationException"/>
    /// with a specified error message and inner exception.
    /// </summary>
    public FargoApplicationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when entities outside the actor's partition access are returned.
/// </summary>
public class EntityAccessViolationFargoApplicationException(Guid actorGuid)
    : FargoApplicationException($"Query returned entities outside the partition access of actor '{actorGuid}'.")
{
    /// <summary>
    /// Gets the identifier of the actor whose partition access was violated.
    /// </summary>
    public Guid ActorGuid { get; } = actorGuid;
}

#endregion Exceptions
