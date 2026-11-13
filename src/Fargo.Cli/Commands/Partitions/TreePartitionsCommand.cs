using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

/// <summary>
/// Renders partitions as a tree, nesting each partition under its parent.
/// </summary>
/// <remarks>
/// A partition whose parent is not among the visible partitions appears at the root level.
/// </remarks>
public sealed class TreePartitionsCommand : Command
{
    public TreePartitionsCommand(FargoApiClient client)
        : base("tree", """
            Show partitions as a tree: child partitions nested under their parent.

            Each line is tagged with its type ([partition]). Partitions with no
            parent (or whose parent is not visible) appear at the root level.

            Examples:
              fargo partition tree
            """)
    {
        SetAction(async (_, cancellationToken) =>
        {
            var partitions = (await client.V1.Partitions.GetAsync(cancellationToken: cancellationToken))?.Items ?? [];

            var partitionsByGuid = new Dictionary<Guid, PartitionDto>();
            var childrenByParent = new Dictionary<Guid, List<PartitionDto>>();
            var rootPartitions = new List<PartitionDto>();

            foreach (var partition in partitions)
            {
                if (partition.Guid is { } guid)
                {
                    partitionsByGuid[guid] = partition;
                }
            }

            foreach (var partition in partitions)
            {
                var parentGuid = partition.ParentPartitionGuid;

                // A partition is a root when it has no parent, or its parent is
                // not among the partitions we can see (orphaned -> treat as root).
                if (parentGuid is { } pg && partitionsByGuid.ContainsKey(pg))
                {
                    if (!childrenByParent.TryGetValue(pg, out var siblings))
                    {
                        childrenByParent[pg] = siblings = [];
                    }

                    siblings.Add(partition);
                }
                else
                {
                    rootPartitions.Add(partition);
                }
            }

            var roots = new List<TreeNode>();
            var visited = new HashSet<Guid>();

            foreach (var partition in rootPartitions.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
            {
                roots.Add(BuildPartitionNode(partition, childrenByParent, visited));
            }

            if (roots.Count == 0)
            {
                Console.WriteLine("No partitions found.");
                return 0;
            }

            TreeWriter.WriteForest(roots);
            return 0;
        });
    }

    private static TreeNode BuildPartitionNode(
        PartitionDto partition,
        IReadOnlyDictionary<Guid, List<PartitionDto>> childrenByParent,
        HashSet<Guid> visited)
    {
        var node = new TreeNode(PartitionLabel(partition));

        if (partition.Guid is not { } guid || !visited.Add(guid))
        {
            // Missing GUID or already visited (cycle) -> do not recurse.
            return node;
        }

        if (childrenByParent.TryGetValue(guid, out var children))
        {
            foreach (var child in children.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
            {
                node.Add(BuildPartitionNode(child, childrenByParent, visited));
            }
        }

        return node;
    }

    private static string PartitionLabel(PartitionDto partition)
    {
        var name = partition.Name ?? partition.Guid?.ToString() ?? "(unknown)";
        var label = $"[partition] {name}";

        if (partition.IsGlobalPartition == true)
        {
            label += " [global]";
        }

        return label;
    }
}
