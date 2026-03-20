using Fargo.Application.Models.UserModels;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Client.Clients.Serialization;
using Fargo.Web.Api;
using System.Net;

namespace Fargo.Web.Features.Users;

public sealed class UserApi(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

    public async Task<IReadOnlyList<UserInformation>> GetManyAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var response = await client.GetAsync("/users", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return [];
        }

        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<UserInformation>>(FargoJsonSerializerOptions.Default, cancellationToken)
            ?? [];
    }

    public async Task<UserInformation?> GetSingleAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var response = await client.GetAsync($"/users/{userGuid}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<UserInformation>(FargoJsonSerializerOptions.Default, cancellationToken);
    }

    public async Task CreateAsync(UserCreateModel model, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var response = await client.PostAsJsonAsync("/users", model, FargoJsonSerializerOptions.Default, cancellationToken);
        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateAsync(Guid userGuid, UserUpdateModel model, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/users/{userGuid}")
        {
            Content = JsonContent.Create(model, options: FargoJsonSerializerOptions.Default)
        };

        var response = await client.SendAsync(request, cancellationToken);
        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var response = await client.DeleteAsync($"/users/{userGuid}", cancellationToken);
        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);
    }
}
