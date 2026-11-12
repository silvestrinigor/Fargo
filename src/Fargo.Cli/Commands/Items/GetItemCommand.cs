using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class GetItemCommand : Command
{
    public GetItemCommand(FargoApiClient client)
        : base("get", """
            Show the full details of a single item.

            Examples:
              fargo item get 22222222-2222-2222-2222-222222222222
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var item = await client.V1.Items[guid].GetAsync(cancellationToken: cancellationToken);

            if (item is null)
            {
                Console.Error.WriteLine("Item not found.");
                return 1;
            }

            var partitions = item.Partitions is { Count: > 0 }
                ? string.Join(", ", item.Partitions.Where(p => p.HasValue).Select(p => p!.Value))
                : null;

            new DetailWriter()
                .Title("Item")
                .Field("GUID", item.Guid)
                .Field("Article", item.ArticleGuid)
                .Field("Container", item.ParentContainerGuid)
                .Field("Fixed", item.IsFixed)
                .Field("Partitions", partitions)
                .Write();

            return 0;
        });
    }
}
