namespace Fargo.Application.Common;

/// <summary>
/// Provides access to client context information, specifically the IP address of the requesting client.
/// </summary>
public interface IClientContext
{
    /// <summary>
    /// Gets the IP address of the client making the request.
    /// </summary>
    string IpAddress { get; }
}
