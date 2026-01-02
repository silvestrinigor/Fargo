namespace Fargo.Domain.Entities
{
    public class Item
    {
        public Guid Guid { get; init; } = Guid.NewGuid();

        public DateTime CreatedAt { get; init; } = DateTime.Now;

        public required Article Article { get; init; }
    }
}
