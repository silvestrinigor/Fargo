using Fargo.Domain.Entities;

namespace Fargo.Domain.Events
{
    public sealed class ItemCreatedEventArgs(Item item) : EventArgs
    {
        public Guid ItemGuid { get; } = item.Guid;
        public Item Item { get; } = item;
    }
}
