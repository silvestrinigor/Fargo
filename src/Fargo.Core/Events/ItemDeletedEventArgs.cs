using Fargo.Domain.Entities.Models;

namespace Fargo.Domain.Events
{
    public class ItemDeletedEventArgs(Item item) : EventArgs
    {
        public Item Item { get; } = item;
    }
}
