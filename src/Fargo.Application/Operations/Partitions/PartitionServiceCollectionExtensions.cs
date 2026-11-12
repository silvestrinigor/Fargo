using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.Partitions.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Partitions;

public static class PartitionServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoPartitionApplication() => services
        .AddFargoCommandHandler<PartitionCreateCommand, Guid, PartitionCreateCommandHandler>()
        .AddFargoCommandHandler<PartitionUpdateCommand, PartitionUpdateCommandHandler>()
        .AddFargoCommandHandler<PartitionDeleteCommand, PartitionDeleteCommandHandler>()
        .AddFargoQueryHandler<PartitionSingleQuery, PartitionDto?, PartitionSingleQueryHandler>()
        .AddFargoQueryHandler<PartitionsQuery, PagedResult<PartitionDto>, PartitionsQueryHandler>();
    }
}
