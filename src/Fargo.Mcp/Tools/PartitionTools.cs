using Fargo.Api.Partitions;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class PartitionTools(IPartitionHttpClient partitions)
{
    [McpServerTool(Name = "list_partitions"), Description("Lists partitions. Omit parentPartitionGuid to list root partitions; provide it to list direct children.")]
    public async Task<string> ListPartitions(
        [Description("GUID of the parent partition to list children of. Omit to list root partitions.")] string? parentPartitionGuid = null)
    {
        Guid? parentGuid = parentPartitionGuid is not null ? Guid.Parse(parentPartitionGuid) : null;
        var response = await partitions.GetManyAsync(
            parentPartitionGuid: parentGuid,
            rootOnly: parentGuid is null ? true : null);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var list = response.Data!.Select(p => new { p.Guid, p.Name, p.Description, p.ParentPartitionGuid, p.IsActive }).ToList();
        return JsonSerializer.Serialize(list);
    }

    [McpServerTool(Name = "get_partition"), Description("Gets a single partition by its GUID.")]
    public async Task<string> GetPartition(
        [Description("The GUID of the partition.")] string partitionGuid)
    {
        var response = await partitions.GetAsync(Guid.Parse(partitionGuid));
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var partition = response.Data!;
        return JsonSerializer.Serialize(new { partition.Guid, partition.Name, partition.Description, partition.ParentPartitionGuid, partition.IsActive });
    }

    [McpServerTool(Name = "create_partition"), Description("Creates a new partition.")]
    public async Task<string> CreatePartition(
        [Description("The display name of the partition.")] string name,
        [Description("An optional description.")] string? description = null,
        [Description("GUID of the parent partition. Omit to create a top-level partition.")] string? parentPartitionGuid = null)
    {
        Guid? parentGuid = parentPartitionGuid is not null ? Guid.Parse(parentPartitionGuid) : null;
        var response = await partitions.CreateAsync(name, description, parentGuid);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        return $"Created partition with GUID: {response.Data}";
    }

    [McpServerTool(Name = "update_partition"), Description("Updates a partition's name and/or description.")]
    public async Task<string> UpdatePartition(
        [Description("The GUID of the partition to update.")] string partitionGuid,
        [Description("The new name. Omit to leave unchanged.")] string? name = null,
        [Description("The new description. Omit to leave unchanged.")] string? description = null)
    {
        var response = await partitions.UpdateAsync(Guid.Parse(partitionGuid), name, description);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        return "Partition updated successfully.";
    }

    [McpServerTool(Name = "delete_partition"), Description("Deletes a partition. The global root partition cannot be deleted.")]
    public async Task<string> DeletePartition(
        [Description("The GUID of the partition to delete.")] string partitionGuid)
    {
        var response = await partitions.DeleteAsync(Guid.Parse(partitionGuid));
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        return "Partition deleted successfully.";
    }
}
