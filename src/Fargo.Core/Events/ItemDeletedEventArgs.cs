using Fargo.Domain.Entities;

namespace Fargo.Domain.Events
{
    public class ItemDeletedEventArgs(Item item) : EventArgs
    {
        public Guid ItemGuid { get; } = item.Guid;
    }
}
