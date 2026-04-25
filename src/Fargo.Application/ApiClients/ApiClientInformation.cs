namespace Fargo.Application.ApiClients;

public sealed record ApiClientInformation(
    Guid Guid,
    string Name,
    string Description,
    bool IsActive,
    Guid? EditedByGuid = null
);
