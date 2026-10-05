using Fargo.Application.Common;

namespace Fargo.Application.Items;

public sealed record ItemCreateCommand(ItemCreateDto Create) : ICommand<Guid>;
