using Fargo.Domain.Abstracts.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class Item : Entity
    {
        public required Article Article { get; init; }
        public Item? Container
        {
            get;
            private set
            {
                if (value != null && !value.Article.IsContainer) throw new InvalidOperationException("This item is not a container.");
                field = value;
            }
        }
        public IEnumerable<Item>? ContainedItems => Article.IsContainer ? containedItems.AsReadOnly() : null;
        private readonly List<Item> containedItems = [];
        public event EventHandler? ItemAdded;
        public event EventHandler? ItemRemoved;
        public Mass ContainedMass { get; private set;} = Mass.Zero;
        private void OnItemAdded()
        {
            ItemAdded?.Invoke(this, EventArgs.Empty);
        }

        private void OnItemRemoved()
        {
            ItemRemoved?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Add(Item item)
        {
            if (!this.Article.IsContainer) throw new InvalidOperationException("This item is not a container.");
            if (item.Container == this) return;
            if (item == this) throw new InvalidOperationException("Cannot add entity to itself.");
            if (item.Container != this.Container) throw new InvalidOperationException("Entity is in a different parent or inside a child entity.");
            if (this.Article.ContainerMassCapacity.HasValue)
            {
                if (!item.Article.Mass.HasValue)
                    throw new InvalidOperationException("Cannot add item with undefined mass to container with defined mass capacity.");
                
                if (ContainedMass + item.Article.Mass + item.ContainedMass > this.Article.ContainerMassCapacity)
                    throw new InvalidOperationException("Adding this item would exceed the container's mass capacity.");
                
                ContainedMass += item.Article.Mass.Value + item.ContainedMass;
            }
            containedItems.Add(item);
            item.Container = this;
            OnItemAdded();
        }

        public virtual void Remove(Item item)
        {
            if (!this.Article.IsContainer) throw new InvalidOperationException("This item is not a container.");
            if (item.Container != this) throw new InvalidOperationException("Entity is in a child entity or outside of this container.");
            if (this.Article.ContainerMassCapacity.HasValue && item.Article.Mass.HasValue)
            {
                ContainedMass -= item.Article.Mass.Value + item.ContainedMass;
            }
            containedItems.Remove(item);
            item.Container = this.Container;
            OnItemRemoved();
        }
    }
}