using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Commands.Items;
using Fargo.Application.Dtos.Items;
using Fargo.Application.Queries.Items;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class ItemServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoItemApplication() => services
        .AddFargoCommandHandler<ItemCreateCommand, Guid, ItemCreateCommandHandler>()
        .AddFargoCommandHandler<ItemSetPartitionsCommand, ItemSetPartitionsCommandHandler>()
        .AddFargoCommandHandler<ItemSetLocationCommand, ItemSetLocationCommandHandler>()
        .AddFargoCommandHandler<ItemRemoveFromContainerCommand, ItemRemoveFromContainerCommandHandler>()
        .AddFargoCommandHandler<ItemDeleteCommand, ItemDeleteCommandHandler>()
        .AddFargoCommandHandler<ItemEventCreateCommand, Guid, ItemEventCreateCommandHandler>()
        .AddFargoQueryHandler<ItemSingleQuery, ItemDto?, ItemSingleQueryHandler>()
        .AddFargoQueryHandler<ItemsQuery, PagedResult<ItemDto>, ItemsQueryHandler>()
        .AddFargoQueryHandler<ItemLocationQuery, IReadOnlyCollection<ItemDto>, ItemLocationQueryHandler>()
        .AddFargoQueryHandler<ItemMovimentsQuery, IReadOnlyCollection<ItemMovimentDto>?, ItemMovimentsQueryHandler>()
        .AddFargoQueryHandler<ItemEventsQuery, PagedResult<ItemEventDto>, ItemEventsQueryHandler>()
        .AddFargoQueryHandler<ItemContainerInventoryByGuidQuery, IReadOnlyCollection<ItemContainerInventoryDto>?, ItemContainerInventoryQueryHandler>();
    }
}
