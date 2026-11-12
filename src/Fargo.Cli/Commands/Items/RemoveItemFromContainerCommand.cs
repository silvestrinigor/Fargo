using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class RemoveItemFromContainerCommand : Command
{
    public RemoveItemFromContainerCommand(FargoApiClient client)
        : base("remove-from-container", """
            Remove an item from its current container.

            This is a separate operation from update and requires its own permission.

            Examples:
              fargo item remove-from-container 22222222-2222-2222-2222-222222222222
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

            await client.V1.Items[guid].Location.DeleteAsync(cancellationToken: cancellationToken);

            StatusWriter.Success("Item removed from its container.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}