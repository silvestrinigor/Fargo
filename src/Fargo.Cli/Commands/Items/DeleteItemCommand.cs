using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class DeleteItemCommand : Command
{
    public DeleteItemCommand(FargoApiClient client)
        : base("delete", "Delete an item by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.Items[guid].DeleteAsync();

            Console.WriteLine("Item deleted.");

            return 0;
        });
    }
}
