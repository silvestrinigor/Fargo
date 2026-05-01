using Fargo.Domain.ClientApplications;
using System.Linq.Expressions;

namespace Fargo.Application.ApiClients;

/// <summary>Mapping helpers between <see cref="ClientApplication"/> domain entities and <see cref="ApiClientInformation"/> read-models.</summary>
public static class ApiClientMappings
{
    /// <summary>An EF-compatible projection expression from <see cref="ClientApplication"/> to <see cref="ApiClientInformation"/>.</summary>
    public static readonly Expression<Func<ClientApplication, ApiClientInformation>> InformationProjection =
        c => new ApiClientInformation(
            c.Guid,
            c.Name,
            c.Description,
            c.IsActive,
            c.EditedByGuid
        );

    /// <summary>Maps an <see cref="ClientApplication"/> domain entity to an <see cref="ApiClientInformation"/> read-model.</summary>
    /// <param name="c">The API client entity to map.</param>
    public static ApiClientInformation ToInformation(this ClientApplication c)
    {
        ArgumentNullException.ThrowIfNull(c);
        return new ApiClientInformation(c.Guid, c.Name, c.Description, c.IsActive, c.EditedByGuid);
    }
}
