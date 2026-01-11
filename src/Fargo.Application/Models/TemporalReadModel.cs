namespace Fargo.Application.Models
{
    public sealed record TemporalReadModel<TEntity>(
        TEntity Entity,
        DateTime PeriodStart,
        DateTime PeriodEnd
        ) where TEntity : IEntityByGuidReadModel, IEntityTemporalReadModel;
}