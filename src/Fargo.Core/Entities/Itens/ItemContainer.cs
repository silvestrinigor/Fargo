using UnitsNet;

namespace Fargo.Domain.Entities.Itens
{
    public partial class Item
    {
        public Mass ContainedMass { get; private set; } = Mass.Zero;
        public Volume ContainedVolume { get; private set; } = Volume.Zero;

        public void Add(Item item)
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
            }

            if (this.Article.ContainerVolumeCapacity.HasValue)
            {
                if (!item.Article.Volume.HasValue)
                    throw new InvalidOperationException("Cannot add item with undefined volume to container with defined volume capacity.");

                if (ContainedVolume + item.Article.Volume + item.ContainedVolume > this.Article.ContainerVolumeCapacity)
                    throw new InvalidOperationException("Adding this item would exceed the container's volume capacity.");
            }

            ContainedMass += item.Article.Mass ?? Mass.Zero + item.ContainedMass;
            ContainedVolume += item.Article.Volume ?? Volume.Zero + item.ContainedVolume;
            item.Container = this;
            OnItemAdded();
        }

        public void Remove(Item item)
        {
            if (!this.Article.IsContainer) throw new InvalidOperationException("This item is not a container.");

            if (item.Container != this) throw new InvalidOperationException("Entity is in a child entity or outside of this container.");
            
            ContainedMass -= item.Article.Mass ?? Mass.Zero + item.ContainedMass;
            ContainedVolume -= item.Article.Volume ?? Volume.Zero + item.ContainedVolume;
            item.Container = this.Container;
            OnItemRemoved();
        }

        public event EventHandler? ItemAdded;

        public event EventHandler? ItemRemoved;
        
        private void OnItemAdded()
        {
            ItemAdded?.Invoke(this, EventArgs.Empty);
        }

        private void OnItemRemoved()
        {
            ItemRemoved?.Invoke(this, EventArgs.Empty);
        }
    }
}
