namespace Fargo.Application.Requests.Queries
{
    /// <summary>
    /// Provides a query interface that returns a response.
    /// </summary>
    /// <typeparam name="TResponse">The response type of the query.</typeparam>
    public interface IQuery<out TResponse>;
}