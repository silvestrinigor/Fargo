using Fargo.Domain.Entities.Articles;
using UnitsNet;

namespace Fargo.Domain.Entities.Itens
{
    public class Container
    {
        public required Article Article 
        { 
            get;
            init
            {
                field = value.IsContainer
                    ? value : throw new InvalidOperationException("Article must be a ContainerArticle.");
            }
        }

        public void Add(Item item)
        {
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

    }
}
