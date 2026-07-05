namespace Fargo.Application;

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
