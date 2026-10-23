using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class GetArticleByBarcodeCommand : Command
{
    public GetArticleByBarcodeCommand(FargoApiClient client)
        : base("get-by-barcode", """
            Look up a single article by one of its barcodes.

            The barcode is given as {value}:{type}.

            Examples:
              fargo article get-by-barcode 1234567890128:ean13
            """)
    {
        var barcodeArgument = new Argument<string>("barcode")
        {
            Description = "The barcode in {value}:{type} format (e.g. 1234567890128:ean13)."
        };

        Add(barcodeArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var barcode = parseResult.GetValue(barcodeArgument)!;

            var article = await client.Articles[barcode].GetAsync(cancellationToken: cancellationToken);

            if (article is null)
            {
                Console.Error.WriteLine("Article not found.");
                return 1;
            }

            ArticleDetailRenderer.Write(article);

            return 0;
        });
    }
}
