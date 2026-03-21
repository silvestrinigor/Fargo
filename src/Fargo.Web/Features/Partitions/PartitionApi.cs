using Fargo.Application.Models.PartitionModels;
using Fargo.Domain.ValueObjects;
using Fargo.Web.Api;

namespace Fargo.Web.Features.Partitions;

public sealed class PartitionApi(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor)
    : FargoApiClientBase(httpClientFactory, sessionAccessor)
{
    public async Task<IReadOnlyCollection<PartitionSummary>> GetChildrenAsync(
        Guid? parentPartitionGuid,
        CancellationToken cancellationToken = default)
    {
        var uri = $"/partitions?parentPartitionGuid={parentPartitionGuid}";

        var result = await CreateClient()
            .GetFromJsonAsync<IReadOnlyCollection<PartitionInformation>>(uri, cancellationToken);

        return result?.Select(ToSummary).ToArray()
            ?? Array.Empty<PartitionSummary>();
    }

    public async Task<PartitionSummary?> GetSingleAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var partition = await CreateClient()
            .GetFromJsonAsync<PartitionInformation>($"/partitions/{partitionGuid}", cancellationToken);

        return partition is null ? null : ToSummary(partition);
    }

    public async Task<Guid> CreateAsync(
        string name,
        string? description,
        Guid parentPartitionGuid,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            name,
            description,
            parentPartitionGuid
        };

        using var response = await CreateClient()
            .PostAsJsonAsync("/partitions", payload, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task UpdateAsync(
        Guid partitionGuid,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var model = new PartitionUpdateModel(
            string.IsNullOrWhiteSpace(description) ? null : new Description(description));

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/partitions/{partitionGuid}")
        {
            Content = JsonContent.Create(model)
        };

        using var response = await CreateClient()
            .SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateClient()
            .DeleteAsync($"/partitions/{partitionGuid}", cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private static PartitionSummary ToSummary(PartitionInformation partition) => new(
        partition.Guid,
        partition.Name.Value,
        partition.Description.Value,
        partition.ParentPartitionGuid,
        partition.IsActive);
}
