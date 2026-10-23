using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class DeleteArticleByBarcodeCommand : Command
{
    public DeleteArticleByBarcodeCommand(FargoApiClient client)
        : base("delete-by-barcode", """
            Delete an article addressed by one of its barcodes.

            The barcode is given as {value}:{type} (e.g. 1234567890128:ean13).

            Examples:
              fargo article delete-by-barcode 1234567890128:ean13
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

            await client.Articles[barcode].DeleteAsync(cancellationToken: cancellationToken);

            StatusWriter.Success("Article deleted.", ("Barcode", barcode));

            return 0;
        });
    }
}
