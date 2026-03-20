using Fargo.Application.Commands.PartitionCommands;
using Fargo.Application.Models.PartitionModels;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Client.Clients.Serialization;
using Fargo.Web.Api;
using System.Net;
using System.Net.Http.Json;

namespace Fargo.Web.Features.Partitions;

public sealed class PartitionApi(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

    public async Task<IReadOnlyList<PartitionSummary>> GetChildrenAsync(Guid? parentPartitionGuid, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var uri = parentPartitionGuid.HasValue
            ? $"/partitions?parentPartitionGuid={parentPartitionGuid.Value}"
            : "/partitions";

        var response = await client.GetAsync(uri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return [];
        }

        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PartitionSummary>>(FargoJsonSerializerOptions.Default, cancellationToken)
            ?? [];
    }

    public async Task<PartitionSummary?> GetSingleAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var response = await client.GetAsync($"/partitions/{partitionGuid}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<PartitionSummary>(FargoJsonSerializerOptions.Default, cancellationToken);
    }

    public async Task<Guid> CreateAsync(string name, string? description, Guid? parentPartitionGuid, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var command = new PartitionCreateCommand(
            new Name(name),
            string.IsNullOrWhiteSpace(description) ? null : new Description(description),
            parentPartitionGuid);

        var response = await client.PostAsJsonAsync("/partitions", command, FargoJsonSerializerOptions.Default, cancellationToken);
        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);

        return (await response.Content.ReadFromJsonAsync<Guid>(FargoJsonSerializerOptions.Default, cancellationToken));
    }

    public async Task UpdateAsync(Guid partitionGuid, string? description, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var model = new PartitionUpdateModel(
            string.IsNullOrWhiteSpace(description) ? null : new Description(description));

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/partitions/{partitionGuid}")
        {
            Content = JsonContent.Create(model, options: FargoJsonSerializerOptions.Default)
        };

        var response = await client.SendAsync(request, cancellationToken);
        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var response = await client.DeleteAsync($"/partitions/{partitionGuid}", cancellationToken);
        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);
    }
}
