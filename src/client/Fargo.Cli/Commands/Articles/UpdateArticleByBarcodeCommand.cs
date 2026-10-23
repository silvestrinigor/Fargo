using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class UpdateArticleByBarcodeCommand : Command
{
    public UpdateArticleByBarcodeCommand(FargoApiClient client)
        : base("update-by-barcode", """
            Update an existing article addressed by one of its barcodes. Only the
            options you pass are changed.

            The barcode is given as {value}:{type} (e.g. 1234567890128:ean13).
            Measures are written as a number followed by a unit, e.g. '2.5kg'.

            Examples:
              fargo article update-by-barcode 1234567890128:ean13 --name "New name"
              fargo article update-by-barcode 1234567890128:ean13 --mass 3kg --remove-shelf-life
            """)
    {
        var barcodeArgument = new Argument<string>("barcode")
        {
            Description = "The barcode in {value}:{type} format (e.g. 1234567890128:ean13)."
        };

        var options = new ArticleUpdateOptions();

        Add(barcodeArgument);
        options.AddTo(this);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var barcode = parseResult.GetValue(barcodeArgument)!;

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

            await client.Articles[barcode].PatchAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Article updated.", ("Barcode", barcode));

            return 0;
        });
    }
}
