using Fargo.Application.Shared.Items;

namespace Fargo.Application.Items;

public sealed record ItemCreateCommand(
    ItemCreateDto Create
) : ICommand<Guid>;