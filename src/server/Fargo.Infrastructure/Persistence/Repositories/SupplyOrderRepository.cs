using Fargo.Application.Common.Paging;
using Fargo.Application.Operations.SupplyOrders;
using Fargo.Application.Operations.SupplyOrders.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Fargo.Infrastructure.Persistence.Repositories;

public sealed class SupplyOrderRepository(FargoDbContext context)
    : ISupplyOrderRepository, ISupplyOrderQueryRepository
{
    public void Add(SupplyOrder supplyOrder) => context.SupplyOrders.Add(supplyOrder);

    public void Remove(SupplyOrder supplyOrder) => context.SupplyOrders.Remove(supplyOrder);

    public Task<SupplyOrder?> GetByGuidAsync(Guid supplyOrderGuid, CancellationToken cancellationToken = default)
    {
        return context.SupplyOrders
            .Include(order => order.Lines)
                .ThenInclude(line => line.Article)
            .Include(order => order.Partitions)
            .Include(order => order.ReceivingContainer)
            .SingleOrDefaultAsync(order => order.Guid == supplyOrderGuid, cancellationToken);
    }

    public Task<SupplyOrderDto?> GetInfoByGuidAsync(
        Guid supplyOrderGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        return ApplyPartitionFilter(context.SupplyOrders.AsNoTracking(), childOfAnyOfThesePartitions)
            .Where(order => order.Guid == supplyOrderGuid)
            .Select(SupplyOrderDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static readonly IReadOnlyDictionary<string, Expression<Func<SupplyOrder, object?>>> SupplyOrderSortFields =
        new Dictionary<string, Expression<Func<SupplyOrder, object?>>>
        {
            ["guid"] = o => o.Guid,
            ["reference"] = o => o.Reference,
            ["status"] = o => o.Status,
            ["createdat"] = o => o.CreatedAt,
            ["placedat"] = o => o.PlacedAt,
        };

    public async Task<PagedResult<SupplyOrderDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        SupplyOrderStatus? status = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(context.SupplyOrders.AsNoTracking(), childOfAnyOfThesePartitions);

        if (status is { } statusFilter)
        {
            queryFiltered = queryFiltered.Where(order => order.Status == statusFilter);
        }

        queryFiltered = ApplySearch(queryFiltered, search);

        var totalCount = await queryFiltered.CountAsync(cancellationToken);

        var orders = await queryFiltered
            .ApplySort(sort, SupplyOrderSortFields, o => o.CreatedAt, o => o.Guid)
            .WithPagination(pagination)
            .Select(SupplyOrderDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<SupplyOrderDto>(
            orders,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    private static IQueryable<SupplyOrder> ApplyPartitionFilter(
        IQueryable<SupplyOrder> query,
        IReadOnlyCollection<Guid>? partitionGuids)
        => query.WherePartitionAccessible(
            partitionGuids,
            order => order.Partitions,
            partition => partition.PartitionGuid);

    private static IQueryable<SupplyOrder> ApplySearch(IQueryable<SupplyOrder> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var pattern = $"%{SearchPattern.Escape(search.Trim())}%";

        return query.Where(o =>
            EF.Functions.ILike((string)o.Reference, pattern, SearchPattern.EscapeChar)
            || EF.Functions.ILike((string)o.Description, pattern, SearchPattern.EscapeChar));
    }
}
