using Fargo.Application.Shared.Users;

namespace Fargo.HttpClient;

public interface IFargoUserClient
{
    Task<UserDto?> GetAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        UserCreateDto request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid userGuid,
        UserUpdateDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default);
}

internal sealed class FargoUserClient(FargoHttpTransport transport) : IFargoUserClient
{
    public Task<UserDto?> GetAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
        => transport.SendNullableAsync<UserDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath(
                $"/users/{userGuid:D}",
                FargoHttpTransport.SingleQuery(temporalAsOf)),
            null,
            cancellationToken);

    public Task<IReadOnlyCollection<UserDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default)
        => transport.SendCollectionAsync<UserDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath("/users/", FargoHttpTransport.ListQuery(query)),
            null,
            cancellationToken);

    public Task<Guid> CreateAsync(
        UserCreateDto request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<Guid>(
            HttpMethod.Post,
            "/users/",
            request,
            cancellationToken);

    public Task UpdateAsync(
        Guid userGuid,
        UserUpdateDto request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Put,
            $"/users/{userGuid:D}",
            request,
            cancellationToken);

    public Task DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Delete,
            $"/users/{userGuid:D}",
            null,
            cancellationToken);
}
