using Fargo.Domain.ApiClients;
using System.Linq.Expressions;

namespace Fargo.Application.ApiClients;

public static class ApiClientMappings
{
    public static readonly Expression<Func<ApiClient, ApiClientInformation>> InformationProjection =
        c => new ApiClientInformation(
            c.Guid,
            c.Name,
            c.Description,
            c.IsActive,
            c.EditedByGuid
        );

    public static ApiClientInformation ToInformation(this ApiClient c)
    {
        ArgumentNullException.ThrowIfNull(c);
        return new ApiClientInformation(c.Guid, c.Name, c.Description, c.IsActive, c.EditedByGuid);
    }
}
