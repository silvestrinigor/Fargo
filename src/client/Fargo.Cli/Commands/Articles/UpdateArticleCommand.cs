using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class UpdateArticleCommand : Command
{
    public UpdateArticleCommand(FargoApiClient client)
        : base("update", """
            Update an existing article by GUID. Only the options you pass are changed.

            Measures are written as a number followed by a unit, e.g. '2.5kg' or
            '10cm'. Use the matching --remove-* flag to clear a value.

            Examples:
              fargo article update 11111111-1111-1111-1111-111111111111 --name "New name"
              fargo article update 11111111-1111-1111-1111-111111111111 --mass 3kg --length-x 12cm
              fargo article update 11111111-1111-1111-1111-111111111111 --remove-mass --remove-barcode
              fargo article update 11111111-1111-1111-1111-111111111111 --barcode-ean13 1234567890128
              fargo article update 11111111-1111-1111-1111-111111111111 --add-partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The article GUID."
        };

        var options = new ArticleUpdateOptions();

        Add(guidArgument);
        options.AddTo(this);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            ClientHttp.Models.ArticleUpdateDto dto;
            try
            {
                dto = options.Build(parseResult);
            }
            catch (FormatException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            await client.Articles[guid.ToString()].PatchAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Article updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
