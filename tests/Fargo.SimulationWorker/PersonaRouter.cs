namespace Fargo.SimulationWorker;

/// <summary>
/// Chooses the next action for an operator according to its role. Each role has
/// a weighted menu of operations so its behaviour resembles a real warehouse
/// worker: a receiver mostly creates stock, a picker mostly moves and ships it,
/// a bulk handler mostly records movement events, and an auditor only reads.
/// </summary>
internal sealed class PersonaRouter(OperatorActions actions)
{
    private readonly OperatorActions _actions = actions;

    /// <summary>A weighted action together with the delegate that performs it.</summary>
    private readonly record struct Weighted(int Weight, Func<OperatorContext, CancellationToken, Task<string>> Run);

    /// <summary>Selects and runs one action for the operator, returning its metric name.</summary>
    public Task<string> RunNextAsync(OperatorContext ctx, CancellationToken ct)
    {
        var menu = MenuFor(ctx.Account.Role);
        var total = menu.Sum(w => w.Weight);
        var roll = ctx.Random.Next(total);

        var cursor = 0;
        foreach (var entry in menu)
        {
            cursor += entry.Weight;
            if (roll < cursor)
            {
                return entry.Run(ctx, ct);
            }
        }

        // Unreachable, but satisfies the compiler and is a safe default.
        return menu[0].Run(ctx, ct);
    }

    private Weighted[] MenuFor(OperatorRole role) => role switch
    {
        OperatorRole.Receiver =>
        [
            new(3, _actions.CreateProductArticleAsync),
            new(2, _actions.CreateContainerAsync),
            new(5, _actions.CreateItemAsync),
        ],
        OperatorRole.PutAway =>
        [
            new(2, _actions.CreateContainerAsync),
            new(3, _actions.CreateItemAsync),
            new(5, _actions.MoveItemAsync),
        ],
        OperatorRole.Picker =>
        [
            new(2, _actions.CreateItemAsync),
            new(4, _actions.MoveItemAsync),
            new(3, _actions.RemoveItemAsync),
            new(2, _actions.ShipItemAsync),
        ],
        OperatorRole.BulkHandler =>
        [
            new(1, _actions.CreateBulkArticleAsync),
            new(2, _actions.CreateContainerAsync),
            new(5, _actions.BulkReceiptAsync),
            new(4, _actions.BulkTransferAsync),
            new(3, _actions.BulkDispatchAsync),
        ],
        OperatorRole.Auditor =>
        [
            new(1, _actions.AuditAsync),
        ],
        _ =>
        [
            new(1, _actions.CreateItemAsync),
        ],
    };
}
