using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public static class PartitionTools
{
    [McpServerTool(Name = "fargo_partitions_list")]
    [Description("List partitions, optionally paginated and filtered to children of specific partitions.")]
    public static async Task<IReadOnlyList<PartitionSummary>> ListPartitionsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of partitions to return per page.")] int? limit = null,
        [Description("Only return partitions that are children of any of these partition GUIDs.")] Guid[]? partitions = null,
        [Description("Case-insensitive text to match against the partition name or description.")] string? search = null,
        [Description("Sort field, optionally prefixed with '-' for descending (e.g. name, -name). Fields: guid, name, description.")] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.V1.Partitions.GetAsync(request =>
        {
            if (page.HasValue)
            {
                request.QueryParameters.Page = page.Value;
            }

            if (limit.HasValue)
            {
                request.QueryParameters.Limit = limit.Value;
            }

            if (partitions is { Length: > 0 })
            {
                request.QueryParameters.ChildOfAnyOfThesePartitions =
                    [.. partitions.Select(g => (Guid?)g)];
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                request.QueryParameters.Search = search;
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                request.QueryParameters.Sort = sort;
            }
        }, cancellationToken);

        return result?.Items?.Select(p => p.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_partitions_get")]
    [Description("Get a single partition by its GUID.")]
    public static async Task<PartitionSummary?> GetPartitionAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the partition.")] Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var partition = await client.V1.Partitions[partitionGuid].GetAsync(cancellationToken: cancellationToken);

        return partition?.ToSummary();
    }

    [McpServerTool(Name = "fargo_partitions_create")]
    [Description("Create a new partition under an optional parent. Returns the GUID of the created partition.")]
    public static async Task<Guid?> CreatePartitionAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The name of the partition.")] string name,
        [Description("An optional description of the partition.")] string? description = null,
        [Description("The GUID of the parent partition, if this partition should be nested.")] Guid? parentPartitionGuid = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new PartitionCreateDto
        {
            Name = name,
            Description = description,
            ParentPartitionGuid = parentPartitionGuid
        };

        return await client.V1.Partitions.PostAsync(body, cancellationToken: cancellationToken);
    }

    [McpServerTool(Name = "fargo_partitions_delete")]
    [Description("Delete a partition by its GUID.")]
    public static async Task<string> DeletePartitionAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the partition to delete.")] Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.Partitions[partitionGuid].DeleteAsync(cancellationToken: cancellationToken);

        return $"Partition '{partitionGuid}' deleted.";
    }

    [McpServerTool(Name = "fargo_partitions_update")]
    [Description("Update an existing partition's name or description. Only the supplied fields are changed; omitted fields are left as-is. To move the partition, use the set-parent tool.")]
    public static async Task<string> UpdatePartitionAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the partition to update.")] Guid partitionGuid,
        [Description("The new name of the partition. Leave null to keep the existing name.")] string? name = null,
        [Description("The new description of the partition. Leave null to keep the existing description.")] string? description = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new PartitionUpdateDto
        {
            Name = name,
            Description = description
        };

        await client.V1.Partitions[partitionGuid].PatchAsync(body, cancellationToken: cancellationToken);

        return $"Partition '{partitionGuid}' updated.";
    }

    [McpServerTool(Name = "fargo_partitions_set_parent")]
    [Description("Move a partition under another parent, which changes who can access it. Rejects a move that would create a cycle. A partition always has a parent, so it can be moved but not detached. Requires the ChangePartitionParent permission, which is separate from the permission to update.")]
    public static async Task<string> SetPartitionParentAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the partition to move.")] Guid partitionGuid,
        [Description("The GUID of the new parent partition.")] Guid parentPartitionGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.Partitions[partitionGuid].Parent.PutAsync(
            new ParentPartitionDto { ParentPartitionGuid = parentPartitionGuid },
            cancellationToken: cancellationToken);

        return $"Partition '{partitionGuid}' moved under '{parentPartitionGuid}'.";
    }
}
