using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

/// <summary>
/// Shows where an item sits in the container hierarchy as a nested tree, from
/// the outermost container inward to the item itself.
/// </summary>
/// <remarks>
/// The API returns the chain ordered from the item outward to its root
/// container. This command reverses that into a top-down tree so the nesting
/// reads naturally (root first, the queried item last and marked), and prints a
/// compact breadcrumb summary above it.
/// </remarks>
public sealed class ItemLocationCommand : Command
{
    public ItemLocationCommand(FargoApiClient client)
        : base("location", """
            Show where an item is located as a nested container tree, from the
            outermost container inward to the item itself.

            Pass --at to reconstruct the location as it was at a specific
            instant, using the item's containment history, instead of the
            current location.

            Examples:
              fargo item location 22222222-2222-2222-2222-222222222222
              fargo item location 22222222-2222-2222-2222-222222222222 --at 2026-03-01T12:00:00Z
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        var atOption = new Option<DateTimeOffset?>("--at")
        {
            Description = "Instant (ISO 8601, e.g. 2026-03-01T12:00:00Z) at which to evaluate the location. Defaults to now."
        };

        Add(guidArgument);
        Add(atOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var at = parseResult.GetValue(atOption);

            var result = await client.Items[guid].Location.GetAsync(request =>
            {
                if (at.HasValue)
                {
                    request.QueryParameters.At = at.Value;
                }
            });

            if (result is null || result.Count == 0)
            {
                Console.WriteLine(at.HasValue
                    ? $"Item '{guid}' had no container location at {at.Value:u}."
                    : $"Item '{guid}' has no container location.");
                return 0;
            }

            Render(guid, at, result);
            return 0;
        });
    }

    private static void Render(Guid queriedGuid, DateTimeOffset? at, IReadOnlyList<ItemDto> chain)
    {
        // The API orders the chain from the item (depth 0) outward to the root
        // container. Reverse it so the root comes first and the item last, which
        // is the natural reading order for a top-down tree.
        var outerToInner = chain.Reverse().ToList();

        Console.WriteLine(at.HasValue
            ? $"Location of item {Short(queriedGuid)} at {at.Value:u}"
            : $"Location of item {Short(queriedGuid)} (current)");
        Console.WriteLine();

        // Breadcrumb: root › ... › item, using short GUIDs for a quick glance.
        var breadcrumb = string.Join(
            " \u203A ",
            outerToInner.Select(i => Short(i.Guid)));
        Console.WriteLine(breadcrumb);
        Console.WriteLine();

        // Nested tree: each container holds the next one inward, ending at the
        // queried item. Depth is counted from the outermost container.
        TreeNode? root = null;
        TreeNode? current = null;

        for (var depth = 0; depth < outerToInner.Count; depth++)
        {
            var item = outerToInner[depth];
            var isItem = item.Guid == queriedGuid;

            var label = BuildLabel(item, depth, isItem);

            if (root is null)
            {
                root = new TreeNode(label);
                current = root;
            }
            else
            {
                current = current!.Add(label);
            }
        }

        TreeWriter.WriteForest([root!]);
    }

    private static string BuildLabel(ItemDto item, int depth, bool isItem)
    {
        // The outermost entry is the root container; inner entries up to the
        // item are nested containers; the final entry is the item itself.
        var role = isItem
            ? "item"
            : depth == 0 ? "root container" : "container";

        var label = $"[{role}] {item.Guid}";

        if (item.ArticleGuid is { } articleGuid)
        {
            label += $"  article={articleGuid}";
        }

        if (item.IsFixed is true)
        {
            label += "  (fixed)";
        }

        if (isItem)
        {
            label += "  \u25C0 this item";
        }

        return label;
    }

    private static string Short(Guid? guid)
    {
        if (guid is not { } g)
        {
            return "-";
        }

        var text = g.ToString();
        return text[..8];
    }
}
