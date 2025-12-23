using Fargo.Domain.Entities.Articles;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Entities
{
    public class Item
    {
        public Guid Guid { get; init; } = Guid.NewGuid();

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public Name? Name { get; set; }

        public Description? Description { get; set; }

        public required PhysicalProductArticle Article { get; init; }

        public Item? Parent { get; private set; }

        public bool IsInContainer => Parent is not null;

        public bool IsLocked { get; private set; } = false;

        public Description? LockReason { get; set; }


        private readonly List<Item> containedItems = [];

        public void InsertIntoContainer(Item container)
        {
            if (container == this)
                throw new InvalidOperationException("Cannot place item into itself.");

            if (!container.Article.IsContainer)
                throw new InvalidOperationException("The specified container item is not a container.");

            if (container.Parent != this.Parent)
                throw new InvalidOperationException("Container should be on the same level.");

            if (container.IsLocked)
                throw new InvalidOperationException("The specified container item is locked.");

            container.containedItems.Add(this);
            Parent = container;
        }

        public void RemoveFromContainer()
        {
            if (Parent is null)
                throw new InvalidOperationException("Item is not in a container.");

            Parent.containedItems.Remove(this);
            Parent = Parent.Parent;
        }

        public void Lock()
        {
            if (Article.IsContainer is false)
                throw new InvalidOperationException("Only container items can be locked and unlocked.");

            IsLocked = true;
        }

        public void Lock(Description reason)
        {
            Lock();
            LockReason = reason;
        }

        public void Unlock()
        {
            if (Article.IsContainer is false)
                throw new InvalidOperationException("Only container items can be locked and unlocked.");

            IsLocked = false;
            LockReason = null;
        }
    }
}
