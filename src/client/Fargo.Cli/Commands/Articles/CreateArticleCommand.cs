using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class CreateArticleCommand : Command
{
    public CreateArticleCommand(FargoApiClient client)
        : base("create", "Create a new article")
    {
        var nameOption = new Option<string>("--name")
        {
            Description = "Name of the article.",
            Required = true
        };

        // ArticleType: 0=Standard, 1=Kit, 2=Pack, 3=Variation (domain values)
        var typeOption = new Option<int>("--type")
        {
            Description = "Article type (0=Standard, 1=Kit, 2=Pack, 3=Variation). Defaults to 0.",
            DefaultValueFactory = x => 0
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Partition GUIDs to associate (space-separated).",
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

            Console.WriteLine($"Article created: {guid}");

            return 0;
        });
    }
}
