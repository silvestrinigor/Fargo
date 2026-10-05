using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class DeleteArticleCommand : Command
{
    public DeleteArticleCommand(FargoApiClient client)
        : base("delete", "Delete an article by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The article GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.Articles[guid.ToString()].DeleteAsync();

            Console.WriteLine("Article deleted.");

            return 0;
        });
    }
}
