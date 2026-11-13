using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class MoveItemCommand : Command
{
    public MoveItemCommand(FargoApiClient client)
        : base("move", """
            Place an item inside a container.

            This is a separate operation from update and requires its own permission.
            To move stock of an event-tracked article, use 'item event move' instead.

            Examples:
              fargo item move 22222222-2222-2222-2222-222222222222 --to 33333333-3333-3333-3333-333333333333
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        var toOption = new Option<Guid>("--to")
        {
            Description = "GUID of the destination container item.",
            Required = true
        };

        Add(guidArgument);
        Add(toOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var to = parseResult.GetValue(toOption);

            await client.V1.Items[guid].Location.PutAsync(
                new ItemSetLocationDto { ParentItemContainerGuid = to },
                cancellationToken: cancellationToken);

            StatusWriter.Success("Item moved.", ("GUID", guid.ToString()), ("Container", to.ToString()));

            return 0;
        });
    }
}
