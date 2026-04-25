namespace Fargo.Application.ApiClients;

/// <summary>Read-model for an API client, returned by query handlers.</summary>
/// <param name="Guid">The unique identifier of the API client.</param>
/// <param name="Name">The display name of the API client.</param>
/// <param name="Description">The description of the API client.</param>
/// <param name="IsActive">Whether the API client is currently active.</param>
/// <param name="EditedByGuid">The identifier of the user who last modified this record, if known.</param>
public sealed record ApiClientInformation(
    Guid Guid,
    string Name,
    string Description,
    bool IsActive,
    Guid? EditedByGuid = null
);
