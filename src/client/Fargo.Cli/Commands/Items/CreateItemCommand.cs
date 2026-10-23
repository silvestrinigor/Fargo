using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class CreateItemCommand : Command
{
    public CreateItemCommand(FargoApiClient client)
        : base("create", """
            Create a new item (an instance of an article).

            Examples:
              fargo item create --article 11111111-1111-1111-1111-111111111111
              fargo item create --article 11111111-1111-1111-1111-111111111111 --container 22222222-2222-2222-2222-222222222222
            """)
    {
        var articleOption = new Option<Guid?>("--article")
        {
            Description = "GUID of the article this item is an instance of."
        };

        var containerOption = new Option<Guid?>("--container")
        {
            Description = "GUID of the parent container item."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Partition GUIDs to associate (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(articleOption);
        Add(containerOption);
        Add(partitionsOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var article = parseResult.GetValue(articleOption);
            var container = parseResult.GetValue(containerOption);
            var partitions = parseResult.GetValue(partitionsOption);

            var dto = new ItemCreateDto
            {
                ArticleGuid = article,
                ParentItemContainerGuid = container,
                PartitionsToAdd = partitions?.Select(g => (Guid?)g).ToList()
            };

            var guid = await client.Items.PostAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success(
                "Item created.",
                ("GUID", guid?.ToString()),
                ("Article", article?.ToString()));

            return 0;
        });
    }
}
