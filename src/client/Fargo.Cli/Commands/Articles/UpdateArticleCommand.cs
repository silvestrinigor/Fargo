using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class UpdateArticleCommand : Command
{
    public UpdateArticleCommand(FargoApiClient client)
        : base("update", """
            Update an existing article. Only the options you pass are changed.

            Examples:
              fargo articles update 11111111-1111-1111-1111-111111111111 --name "New name"
              fargo articles update 11111111-1111-1111-1111-111111111111 --add-partitions 00000000-0000-0000-0000-000000000001
            """)
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
                Name = name,
                PartitionsToAdd = addPartitions?.Select(g => (Guid?)g).ToList(),
                PartitionsToRemove = removePartitions?.Select(g => (Guid?)g).ToList()
            };

            await client.Articles[guid.ToString()].PatchAsync(dto);

            StatusWriter.Success("Article updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
