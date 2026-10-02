using Fargo.Http.Client;
using Fargo.Http.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class CreateItemCommand : Command
{
    public CreateItemCommand(FargoApiClient client)
        : base("create", "Create a new item")
    {
        var articleOption = new Option<Guid?>("--article")
        {
            Description = "GUID of the article this item belongs to."
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

        SetAction(async parseResult =>
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

            var guid = await client.Items.PostAsync(dto);

            Console.WriteLine($"Item created: {guid}");

            return 0;
        });
    }
}
