using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

/// <summary>
/// Shows where an item sits in the container hierarchy as a nested tree, outermost container
/// first.
/// </summary>
/// <remarks>
/// The API returns the chain ordered from the item outward to its root container; this command
/// reverses it so the tree nests top-down.
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

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var at = parseResult.GetValue(atOption);

            var result = await client.V1.Items[guid].Location.GetAsync(request =>
            {
                if (at.HasValue)
                {
                    request.QueryParameters.At = at.Value;
                }
            }, cancellationToken);

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
        // Reverse a copy in place: List<T>.Reverse() and LINQ's Enumerable.Reverse() are easy to
        // confuse, and the caller's list must not change.
        var outerToInner = new List<ItemDto>(chain);
        outerToInner.Reverse();

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
