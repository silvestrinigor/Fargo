using Fargo.Application.Models.UserModels;
using Fargo.Domain.ValueObjects;
using Fargo.Web.Api;

namespace Fargo.Web.Features.Users;

public sealed class UserApi(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor)
    : FargoApiClientBase(httpClientFactory, sessionAccessor)
{
    public async Task<IReadOnlyCollection<UserInformation>> GetManyAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await CreateClient()
            .GetFromJsonAsync<IReadOnlyCollection<UserInformation>>("/users", cancellationToken);

        return users ?? Array.Empty<UserInformation>();
    }

    public Task<UserInformation?> GetSingleAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        return CreateClient()
            .GetFromJsonAsync<UserInformation>($"/users/{userGuid}", cancellationToken);
    }

    public async Task CreateAsync(
        UserCreateModel model,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateClient()
            .PostAsJsonAsync("/users", model, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(
        Guid userGuid,
        UserUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/users/{userGuid}")
        {
            Content = JsonContent.Create(model)
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
