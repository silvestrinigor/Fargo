namespace Fargo.Application.Authentication;

/// <summary>
/// Provides the identifier of the API client used to make the current request.
/// </summary>
public interface ICurrentApiClient
{
    /// <summary>Gets the unique identifier of the API client for the current request.</summary>
    Guid ApiClientGuid { get; }
}
