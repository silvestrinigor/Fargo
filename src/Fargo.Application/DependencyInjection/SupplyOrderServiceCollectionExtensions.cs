using Fargo.Application.Commands.SupplyOrders;
using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.SupplyOrders;
using Fargo.Application.Queries.SupplyOrders;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class SupplyOrderServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoSupplyOrderApplication() => services
        .AddFargoCommandHandler<SupplyOrderCreateCommand, Guid, SupplyOrderCreateCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderAddLineCommand, SupplyOrderAddLineCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderChangeLineQuantityCommand, SupplyOrderChangeLineQuantityCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderRemoveLineCommand, SupplyOrderRemoveLineCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderSetReceivingContainerCommand, SupplyOrderSetReceivingContainerCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderPlaceCommand, SupplyOrderPlaceCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderReceiveCommand, SupplyOrderReceiveCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderCancelCommand, SupplyOrderCancelCommandHandler>()
        .AddFargoCommandHandler<SupplyOrderCloseCommand, SupplyOrderCloseCommandHandler>()
        .AddFargoQueryHandler<SupplyOrderSingleQuery, SupplyOrderDto?, SupplyOrderSingleQueryHandler>()
        .AddFargoQueryHandler<SupplyOrdersQuery, PagedResult<SupplyOrderDto>, SupplyOrdersQueryHandler>();
    }
}
