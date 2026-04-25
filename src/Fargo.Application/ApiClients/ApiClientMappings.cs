using Fargo.Domain.ApiClients;
using System.Linq.Expressions;

namespace Fargo.Application.ApiClients;

/// <summary>Mapping helpers between <see cref="ApiClient"/> domain entities and <see cref="ApiClientInformation"/> read-models.</summary>
public static class ApiClientMappings
{
    /// <summary>An EF-compatible projection expression from <see cref="ApiClient"/> to <see cref="ApiClientInformation"/>.</summary>
    public static readonly Expression<Func<ApiClient, ApiClientInformation>> InformationProjection =
        c => new ApiClientInformation(
            c.Guid,
            c.Name,
            c.Description,
            c.IsActive,
            c.EditedByGuid
        );

    /// <summary>Maps an <see cref="ApiClient"/> domain entity to an <see cref="ApiClientInformation"/> read-model.</summary>
    /// <param name="c">The API client entity to map.</param>
    public static ApiClientInformation ToInformation(this ApiClient c)
    {
        ArgumentNullException.ThrowIfNull(c);
        return new ApiClientInformation(c.Guid, c.Name, c.Description, c.IsActive, c.EditedByGuid);
    }
}
