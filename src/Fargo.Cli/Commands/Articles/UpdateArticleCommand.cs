using Fargo.Http.Client;
using Fargo.Http.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class UpdateArticleCommand : Command
{
    public UpdateArticleCommand(FargoApiClient client)
        : base("update", "Update an existing article")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The article GUID."
        };

        var nameOption = new Option<string?>("--name")
        {
            Description = "New name for the article."
        };

        var addPartitionsOption = new Option<Guid[]?>("--add-partitions")
        {
            Description = "Partition GUIDs to add (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removePartitionsOption = new Option<Guid[]?>("--remove-partitions")
        {
            Description = "Partition GUIDs to remove (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(nameOption);
        Add(addPartitionsOption);
        Add(removePartitionsOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var name = parseResult.GetValue(nameOption);
            var addPartitions = parseResult.GetValue(addPartitionsOption);
            var removePartitions = parseResult.GetValue(removePartitionsOption);

            var dto = new ArticleUpdateDto
            {
                Name = name is not null ? name : null,
                PartitionsToAdd = addPartitions?.Select(g => (Guid?)g).ToList(),
                PartitionsToRemove = removePartitions?.Select(g => (Guid?)g).ToList()
            };

            await client.Articles[guid.ToString()].PatchAsync(dto);

            Console.WriteLine("Article updated.");

            return 0;
        });
    }
}
