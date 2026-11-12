using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Commands.Partitions;
using Fargo.Application.Dtos.Partitions;
using Fargo.Application.Queries.Partitions;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class PartitionServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoPartitionApplication() => services
        .AddFargoCommandHandler<PartitionCreateCommand, Guid, PartitionCreateCommandHandler>()
        .AddFargoCommandHandler<PartitionUpdateCommand, PartitionUpdateCommandHandler>()
        .AddFargoCommandHandler<PartitionSetParentCommand, PartitionSetParentCommandHandler>()
        .AddFargoCommandHandler<PartitionDeleteCommand, PartitionDeleteCommandHandler>()
        .AddFargoQueryHandler<PartitionSingleQuery, PartitionDto?, PartitionSingleQueryHandler>()
        .AddFargoQueryHandler<PartitionsQuery, PagedResult<PartitionDto>, PartitionsQueryHandler>();
    }
}
