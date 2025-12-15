namespace Fargo.Domain.Interfaces.Events
{
    public interface IEntityEvent
    {
        Guid AggregateId { get; }
        DateTime OccurredAt { get; }
    }
}
