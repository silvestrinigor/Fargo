using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Core.Items.Events;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemEventRepository(FargoDbContext context) : IItemEventRepository, IItemEventQueryRepository
{
    public void Add(ItemEvent itemEvent)
    {
        context.ItemEvents.Add(itemEvent);
    }

    public async Task<PagedResult<ItemEventDto>> GetManyInfoOrderedByOccuredAtAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Guid? articleGuid = null,
        ItemEventType? eventType = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.ItemEvents.AsNoTracking(),
            childOfAnyOfThesePartitions);

        if (articleGuid is not null)
        {
            queryFiltered = queryFiltered.Where(e => e.ArticleGuid == articleGuid);
        }

        if (eventType is not null)
        {
            queryFiltered = queryFiltered.Where(e => e.EventType == eventType);
        }

        if (periodStart is not null)
        {
            queryFiltered = queryFiltered.Where(e => e.OccuredAt >= periodStart);
        }

        if (periodEnd is not null)
        {
            queryFiltered = queryFiltered.Where(e => e.OccuredAt <= periodEnd);
        }

        var totalCount = await queryFiltered.CountAsync(cancellationToken);

        var itemEvents = await queryFiltered
            .OrderBy(e => e.OccuredAt)
            .Include(e => e.Moviment)
            .Include(e => e.Lifecycle)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        var items = itemEvents.Select(e => e.ToDto()).ToList();

        return new PagedResult<ItemEventDto>(
            items,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    private static IQueryable<ItemEvent> ApplyPartitionFilter(
        IQueryable<ItemEvent> query,
        IReadOnlyCollection<Guid>? partitionGuids)
    {
        if (partitionGuids is null)
        {
            return query;
        }

        return query.Where(itemEvent =>
            itemEvent.Article.Partitions.Any(
                partition => partitionGuids.Contains(partition.PartitionGuid)));
    }
}
