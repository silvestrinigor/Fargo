using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class SetArticlePartitionsCommand : Command
{
    public SetArticlePartitionsCommand(FargoApiClient client)
        : base("set-partitions", """
            Change the partitions of an article.

            This is a separate operation from update and requires the ChangeArticlePartitions
            permission. You need access to every partition you touch.

            Examples:
              fargo article set-partitions 11111111-1111-1111-1111-111111111111 --add 00000000-0000-0000-0000-000000000002
              fargo article set-partitions 11111111-1111-1111-1111-111111111111 --add 00000000-0000-0000-0000-000000000002 --remove 00000000-0000-0000-0000-000000000005
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The an article GUID."
        };

        var addOption = new Option<Guid[]?>("--add")
        {
            Description = "Partition GUIDs to add (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removeOption = new Option<Guid[]?>("--remove")
        {
            Description = "Partition GUIDs to remove (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(addOption);
        Add(removeOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new PartitionMembershipsChangeDto
            {
                PartitionsToAdd = ToGuidList(parseResult.GetValue(addOption)),
                PartitionsToRemove = ToGuidList(parseResult.GetValue(removeOption))
            };

            await client.V1.Articles[guid.ToString()].Partitions.PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Article partitions updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }

    private static List<Guid?>? ToGuidList(Guid[]? values)
        => values is { Length: > 0 } ? [.. values.Select(v => (Guid?)v)] : null;
}
