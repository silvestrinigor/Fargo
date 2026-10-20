using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticleCreateCommand : Command
{
    public ArticleCreateCommand(FargoApiClient client)
        : base("create", """
            Create a new article.

            Examples:
              fargo articles create --name "Widget"
              fargo articles create --name "Widget" --type 2 --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var nameOption = new Option<string>("--name")
        {
            Description = "Name of the article.",
            Required = true
        };

        var typeOption = new Option<int>("--type")
        {
            Description = "Article type as an integer. Defaults to 1.",
            DefaultValueFactory = _ => 1
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Partition GUIDs to associate the article with (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(nameOption);
        Add(typeOption);
        Add(partitionsOption);

        SetAction(async parseResult =>
        {
            var name = parseResult.GetValue(nameOption)!;
            var type = parseResult.GetValue(typeOption);
            var partitions = parseResult.GetValue(partitionsOption);

            var dto = new ArticleCreateDto
            {
                Name = name,
                ArticleType = type,
                PartitionsToAdd = partitions?.Select(g => (Guid?)g).ToList()
            };

            var guid = await client.Articles.PostAsync(dto);

            StatusWriter.Success(
                "Article created.",
                ("GUID", guid?.ToString()),
                ("Name", name));

            return 0;
        });
    }
}
