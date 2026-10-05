using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class GetArticleByBarcodeCommand : Command
{
    public GetArticleByBarcodeCommand(FargoApiClient client)
        : base("get-by-barcode", "Get an article by barcode")
    {
        var barcodeArgument = new Argument<string>("barcode")
        {
            Description = "The barcode value in {value}:{type} format (e.g. 1234567890128:ean13)."
        };

        Add(barcodeArgument);

        SetAction(async parseResult =>
        {
            var barcode = parseResult.GetValue(barcodeArgument)!;

            var article = await client.Articles[barcode].GetAsync();

            if (article is null)
            {
                Console.Error.WriteLine("Article not found.");
                return 1;
            }

            Console.WriteLine($"GUID:  {article.Guid}");
            Console.WriteLine($"Name:  {article.Name}");
            Console.WriteLine($"Type:  {article.ArticleType}");

            return 0;
        });
    }
}
