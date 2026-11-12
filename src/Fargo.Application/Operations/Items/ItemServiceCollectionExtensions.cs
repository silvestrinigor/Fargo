using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.Items.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Items;

public static class ItemServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoItemApplication() => services
        .AddFargoCommandHandler<ItemCreateCommand, Guid, ItemCreateCommandHandler>()
        .AddFargoCommandHandler<ItemUpdateCommand, ItemUpdateCommandHandler>()
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
