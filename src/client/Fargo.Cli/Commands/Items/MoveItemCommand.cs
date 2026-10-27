using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class MoveItemCommand : Command
{
    public MoveItemCommand(FargoApiClient client)
        : base("move", """
            Record a movement of a quantity of an event-tracked article between containers.

            Only valid for articles whose tracking mode is Event. Provide a destination
            (--to) for a receipt, an origin (--from) for a dispatch, or both for a transfer.

            Examples:
              # Receive 50 units into a container
              fargo item move --article 11111111-1111-1111-1111-111111111111 --quantity 50 --to 22222222-2222-2222-2222-222222222222

              # Dispatch 10 units out of a container
              fargo item move --article 11111111-1111-1111-1111-111111111111 --quantity 10 --from 22222222-2222-2222-2222-222222222222

              # Transfer 5 units between containers
              fargo item move --article 11111111-1111-1111-1111-111111111111 --quantity 5 --from 22222222-2222-2222-2222-222222222222 --to 33333333-3333-3333-3333-333333333333
            """)
    {
        var articleOption = new Option<Guid?>("--article")
        {
            Description = "GUID of the event-tracked article being moved.",
            Required = true
        };

        var quantityOption = new Option<double?>("--quantity")
        {
            Description = "The strictly positive quantity moved.",
            Required = true
        };

        var fromOption = new Option<Guid?>("--from")
        {
            Description = "GUID of the origin container. Omit for a receipt from outside any container."
        };

        var toOption = new Option<Guid?>("--to")
        {
            Description = "GUID of the destination container. Omit for a dispatch to outside any container."
        };

        var atOption = new Option<DateTimeOffset?>("--at")
        {
            Description = "Instant (ISO 8601 UTC) at which the movement occurred. Defaults to now."
        };

        Add(articleOption);
        Add(quantityOption);
        Add(fromOption);
        Add(toOption);
        Add(atOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var article = parseResult.GetValue(articleOption);
            var quantity = parseResult.GetValue(quantityOption);
            var from = parseResult.GetValue(fromOption);
            var to = parseResult.GetValue(toOption);
            var at = parseResult.GetValue(atOption);

            var dto = new ItemEventCreateDto
            {
                EventType = (int)ItemEventType.Moviment,
                ArticleGuid = article,
                Quantity = new Scalar { Double = quantity },
                FromItemContainerGuid = from,
                ToItemContainerGuid = to,
                OccuredAt = at
            };

            await client.ItemEvents.PostAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success(
                "Item movement recorded.",
                ("Article", article?.ToString()),
                ("Quantity", quantity?.ToString()),
                ("From", from?.ToString() ?? "(outside)"),
                ("To", to?.ToString() ?? "(outside)"));

            return 0;
        });
    }
}
