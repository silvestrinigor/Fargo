using Fargo.Api.Http;
using Fargo.Api.Partitions;
using Fargo.Sdk;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.Users;

namespace Fargo.Api.Users;

/// <summary>Default implementation of <see cref="IUserHttpClient"/>.</summary>
public sealed class UserHttpClient : IUserHttpClient
{
    /// <summary>Initializes a new instance.</summary>
    public UserHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoSdkResponse<UserResult>> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<UserDto>($"/users/{userGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<UserResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<UserResult>(httpResponse.Data!.ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<UserResult>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, string? search = null, bool? noPartition = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("temporalAsOf", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("partitionGuid", partitionGuid?.ToString()),
            ("search", search),
            ("noPartition", noPartition?.ToString()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<UserDto>>($"/users{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<UserResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<UserResult>>((httpResponse.Data ?? []).ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(string nameid, string password, string? firstName = null, string? lastName = null, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, Guid? firstPartition = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<UserCreateDto, Guid>(
            "/users",
            ContractMappings.ToUserCreateDto(
                nameid,
                password,
                firstName,
                lastName,
                description,
                permissions,
                defaultPasswordExpirationPeriod,
                firstPartition),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid userGuid, string? nameid = null, string? firstName = null, string? lastName = null, string? description = null, string? password = null, bool? isActive = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PatchJsonAsync<UserUpdateDto>(
            $"/users/{userGuid}",
            ContractMappings.ToUserUpdateDto(
                nameid,
                firstName,
                lastName,
                description,
                password,
                isActive,
                permissions,
                defaultPasswordExpirationPeriod),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/users/{userGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> AddUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", new { }, cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> RemoveUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync($"/users/{userGuid}/partitions/{partitionGuid}", new { }, cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/users/{userGuid}/partitions/{partitionGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionDto>>($"/users/{userGuid}/partitions", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>((httpResponse.Data ?? []).ToSdk());
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
