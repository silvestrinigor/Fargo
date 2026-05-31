using Fargo.Application.Shared.UserGroups;

namespace Fargo.HttpClient;

public interface IFargoUserGroupClient
{
    Task<UserGroupDto?> GetAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserGroupDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        UserGroupCreateDto request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid userGroupGuid,
        UserGroupUpdateDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);
}

internal sealed class FargoUserGroupClient(FargoHttpTransport transport) : IFargoUserGroupClient
{
    public Task<UserGroupDto?> GetAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
        => transport.SendNullableAsync<UserGroupDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath(
                $"/user-groups/{userGroupGuid:D}",
                FargoHttpTransport.SingleQuery(temporalAsOf)),
            null,
            cancellationToken);

    public Task<IReadOnlyCollection<UserGroupDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default)
        => transport.SendCollectionAsync<UserGroupDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath("/user-groups/", FargoHttpTransport.ListQuery(query)),
            null,
            cancellationToken);

    public Task<Guid> CreateAsync(
        UserGroupCreateDto request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<Guid>(
            HttpMethod.Post,
            "/user-groups/",
            request,
            cancellationToken);

    public Task UpdateAsync(
        Guid userGroupGuid,
        UserGroupUpdateDto request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Put,
            $"/user-groups/{userGroupGuid:D}",
            request,
            cancellationToken);

    public Task DeleteAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Delete,
            $"/user-groups/{userGroupGuid:D}",
            null,
            cancellationToken);
}
