using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class DeleteArticleCommand : Command
{
    public DeleteArticleCommand(FargoApiClient client)
        : base("delete", """
            Delete an article by GUID.

            Examples:
              fargo article delete 11111111-1111-1111-1111-111111111111
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The article GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.Articles[guid.ToString()].DeleteAsync(cancellationToken: cancellationToken);

            StatusWriter.Success("Article deleted.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
