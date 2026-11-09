using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.SupplyOrders.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.SupplyOrders;

/// <summary>
/// Registers the SupplyOrders use-case handlers.
/// </summary>
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
