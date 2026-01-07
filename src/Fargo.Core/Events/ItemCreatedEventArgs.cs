using Fargo.Domain.Entities.Models;

namespace Fargo.Domain.Events
{
    public sealed class ItemCreatedEventArgs(Item item) : EventArgs
    {
        public Item Item { get; } = item;
    }
}
