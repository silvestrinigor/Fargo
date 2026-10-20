using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class DeleteItemCommand : Command
{
    public DeleteItemCommand(FargoApiClient client)
        : base("delete", """
            Delete an item by GUID.

            Examples:
              fargo items delete 22222222-2222-2222-2222-222222222222
            """)
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

            StatusWriter.Success("Item deleted.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
