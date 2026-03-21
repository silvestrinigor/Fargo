using Fargo.Application.Models.UserModels;
using Fargo.Domain.ValueObjects;
using Fargo.Web.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Fargo.Web.Features.Users;

public sealed class UserApi(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor,
    IOptions<JsonOptions> httpJsonOptions)
    : FargoApiClientBase(httpClientFactory, sessionAccessor, httpJsonOptions)
{
    public async Task<IReadOnlyCollection<UserInformation>> GetManyAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await GetFromJsonAsync<IReadOnlyCollection<UserInformation>>(
            "/users",
            cancellationToken: cancellationToken);

        return users ?? Array.Empty<UserInformation>();
    }

    public Task<UserInformation?> GetSingleAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        return GetFromJsonAsync<UserInformation>(
            $"/users/{userGuid}",
            cancellationToken: cancellationToken);
    }

    public async Task CreateAsync(
        UserCreateModel model,
        CancellationToken cancellationToken = default)
    {
        using var response = await PostAsJsonAsync(
            "/users",
            model,
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(
        Guid userGuid,
        UserUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/users/{userGuid}")
        {
            Content = CreateJsonContent(model)
        };

        using var response = await CreateClient()
            .SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateClient()
            .DeleteAsync($"/users/{userGuid}", cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}