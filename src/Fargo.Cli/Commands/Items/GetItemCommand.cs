using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class GetItemCommand : Command
{
    public GetItemCommand(FargoApiClient client)
        : base("get", "Get a single item by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var item = await client.Items[guid].GetAsync();

            if (item is null)
            {
                Console.Error.WriteLine("Item not found.");
                return 1;
            }

            Console.WriteLine($"GUID:       {item.Guid}");
            Console.WriteLine($"Article:    {item.ArticleGuid}");
            Console.WriteLine($"Container:  {item.ParentContainerGuid}");
            Console.WriteLine($"Fixed:      {item.IsFixed}");

            if (item.Partitions is { Count: > 0 })
            {
                Console.WriteLine($"Partitions: {string.Join(", ", item.Partitions)}");
            }

            return 0;
        });
    }
}
