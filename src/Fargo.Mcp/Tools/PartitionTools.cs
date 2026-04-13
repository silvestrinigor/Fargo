using Fargo.Sdk;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class PartitionTools(IEngine engine)
{
    [McpServerTool(Name = "list_partitions"), Description("Lists partitions. Omit parentPartitionGuid to list root partitions; provide it to list direct children.")]
    public async Task<string> ListPartitions(
        [Description("GUID of the parent partition to list children of. Omit to list root partitions.")] string? parentPartitionGuid = null)
    {
        try
        {
            Guid? parentGuid = parentPartitionGuid is not null ? Guid.Parse(parentPartitionGuid) : null;
            var partitions = await engine.Partitions.GetManyAsync(
                parentPartitionGuid: parentGuid,
                rootOnly: parentGuid is null ? true : null);
            var results = partitions.Select(p => new { p.Guid, p.Name, p.Description, p.ParentPartitionGuid, p.IsActive }).ToList();
            foreach (var p in partitions)
            {
                await p.DisposeAsync();
            }

            return JsonSerializer.Serialize(results);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_partition"), Description("Gets a single partition by its GUID.")]
    public async Task<string> GetPartition(
        [Description("The GUID of the partition.")] string partitionGuid)
    {
        try
        {
            await using var partition = await engine.Partitions.GetAsync(Guid.Parse(partitionGuid));
            return JsonSerializer.Serialize(new { partition.Guid, partition.Name, partition.Description, partition.ParentPartitionGuid, partition.IsActive });
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "create_partition"), Description("Creates a new partition.")]
    public async Task<string> CreatePartition(
        [Description("The display name of the partition.")] string name,
        [Description("An optional description.")] string? description = null,
        [Description("GUID of the parent partition. Omit to create a top-level partition.")] string? parentPartitionGuid = null)
    {
        try
        {
            Guid? parentGuid = parentPartitionGuid is not null ? Guid.Parse(parentPartitionGuid) : null;
            await using var partition = await engine.Partitions.CreateAsync(name, description, parentGuid);
            return $"Created partition with GUID: {partition.Guid}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "update_partition"), Description("Updates a partition's name and/or description.")]
    public async Task<string> UpdatePartition(
        [Description("The GUID of the partition to update.")] string partitionGuid,
        [Description("The new name. Omit to leave unchanged.")] string? name = null,
        [Description("The new description. Omit to leave unchanged.")] string? description = null)
    {
        try
        {
            await using var partition = await engine.Partitions.GetAsync(Guid.Parse(partitionGuid));
            await partition.UpdateAsync(p =>
            {
                if (name is not null)
                {
                    p.Name = name;
                }

                if (description is not null)
                {
                    p.Description = description;
                }
            });
            return "Partition updated successfully.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "delete_partition"), Description("Deletes a partition. The global root partition cannot be deleted.")]
    public async Task<string> DeletePartition(
        [Description("The GUID of the partition to delete.")] string partitionGuid)
    {
        try
        {
            await engine.Partitions.DeleteAsync(Guid.Parse(partitionGuid));
            return "Partition deleted successfully.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
