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
              fargo article create --name "Widget"
              fargo article create --name "Widget" --type pack --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var nameOption = new Option<string>("--name")
        {
            Description = "Name of the article.",
            Required = true
        };

        var typeOption = new Option<ArticleType>("--type")
        {
            Description =
                "Article type by name (default, variation, pack, kit, container). "
                + "Defaults to default.",
            DefaultValueFactory = _ => ArticleType.Default
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
                ArticleType = (int)type,
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
