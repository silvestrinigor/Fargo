using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Core.Items.Events;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemEventRepository(FargoDbContext context) : IItemEventRepository, IItemEventQueryRepository
{
    public void Add(ItemMovimentEvent itemMovimentEvent)
    {
        context.ItemMovimentEvents.Add(itemMovimentEvent);
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
            context.ItemMovimentEvents.AsNoTracking(),
            childOfAnyOfThesePartitions);

        if (articleGuid is not null)
        {
            queryFiltered = queryFiltered.Where(e => e.ItemEvent.ArticleGuid == articleGuid);
        }

        if (eventType is not null)
        {
            queryFiltered = queryFiltered.Where(e => e.ItemEvent.EventType == eventType);
        }

        if (periodStart is not null)
        {
            queryFiltered = queryFiltered.Where(e => e.ItemEvent.OccuredAt >= periodStart);
        }

        if (periodEnd is not null)
        {
            queryFiltered = queryFiltered.Where(e => e.ItemEvent.OccuredAt <= periodEnd);
        }

        var totalCount = await queryFiltered.CountAsync(cancellationToken);

        var movimentEvents = await queryFiltered
            .OrderBy(e => e.ItemEvent.OccuredAt)
            .Include(e => e.ItemEvent)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        var items = movimentEvents.Select(e => e.ToDto()).ToList();

        return new PagedResult<ItemEventDto>(
            items,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    private static IQueryable<ItemMovimentEvent> ApplyPartitionFilter(
        IQueryable<ItemMovimentEvent> query,
        IReadOnlyCollection<Guid>? partitionGuids)
    {
        if (partitionGuids is null)
        {
            return query;
        }

        return query.Where(movimentEvent =>
            movimentEvent.ItemEvent.Article.Partitions.Any(
                partition => partitionGuids.Contains(partition.PartitionGuid)));
    }
}
