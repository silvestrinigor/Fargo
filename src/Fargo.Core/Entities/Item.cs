using Fargo.Domain.ValueObjects;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class Item : Entity
    {
        public Item(Article article, Name? name = null, Description? description = null) : base(name, description)
        {
            Article = article;

            ContainerExtension
                = article.IsContainer
                ? new ItemContainerExtension(this)
                : null;
        }

        public Article Article
        { 
            get;
            private init;
        }

        public bool IsContainer => Article.IsContainer;

        public DateTime? ManufacturedAt
        {
            get;
            init
            {
                if (value > DateTime.Now)
                {
                    throw new ArgumentOutOfRangeException(nameof(ManufacturedAt), "Cannot be in future.");
                }

                field = value;
            }
        } = DateTime.UtcNow;

        public DateTime? ExpirationDate
            => ManufacturedAt + Article.ShelfLife;

        public ItemContainerExtension? ContainerExtension
        {
            get;
            private init
            {
                if (value is not null && !IsContainer)
                {
                    throw new InvalidOperationException("Container extension should be null when item is not a container.");
                }

                if (value is null && IsContainer)
                {
                    throw new InvalidOperationException("Container extension should not be null when item is a container.");
                }

                if (value is not null && value.ItemReference != this)
                {
                    throw new ArgumentException("Container extension item should refer to this.", nameof(ContainerExtension));
                }

                field = value;
            }
        }

        public Item? ParentContainer
        {
            get;
            private set
            {
                if (value == this)
                {
                    throw new ArgumentException("Parent container cannot be equals the child.", nameof(ParentContainer));
                }

                if (!value?.IsContainer ?? false)
                {
                    throw new ArgumentException("Parent container must be a container.", nameof(ParentContainer));
                }

                if (value is not null && value.ParentContainer != this.ParentContainer && value != this.ParentContainer?.ParentContainer)
                {
                    throw new ArgumentException(
                        "The new parent container must be inside the current parent container or be the current grandparent container.", 
                        nameof(ParentContainer));
                }

                field = value;
            }
        }

        public bool IsInContainer => ParentContainer is not null;

        public void InsertIntoContainer(Item container)
        {
            if (container == ParentContainer)
            {
                return;
            }

            if (container.ContainerExtension?.Temperature is null &&
                (this.Article.MinimumContainerTemperature is not null || this.Article.MaximumContainerTemperature is not null))
            {
                throw new ArgumentException(
                    "Parent container must provide temperature data when this item has minimum or maximum container temperature requirements.",
                    nameof(container));
            }

            if (this.Article.MinimumContainerTemperature > container?.ContainerExtension?.Temperature)
            {
                throw new InvalidOperationException("Cannot set container parrent when the container temperature is bellow the item minimum container temperature requirements.");
            }

            if (this.Article.MaximumContainerTemperature < container?.ContainerExtension?.Temperature)
            {
                throw new InvalidOperationException("Cannot set container parrent when the container temperature is bigger than the item maximum container temperature requirements.");
            }

            container?.ContainerExtension?.ValidateAdd(this);

            ParentContainer = container;

            container?.ContainerExtension?.containedItens.Add(this);
        }

        public void RemoveFromContainer()
        {
            if (ParentContainer is null)
            {
                return;
            }

            ParentContainer.ContainerExtension?.containedItens.Remove(this);

            ParentContainer = ParentContainer?.ParentContainer;
        }
    }

    public class ItemContainerExtension
    {
        internal ItemContainerExtension(Item item)
        {
            if (!item.Article.IsContainer)
            {
                throw new InvalidOperationException("Cannot create item container extension when the item is not a container.");
            }

            if (item.Article.ContainerInformation is null)
            {
                throw new InvalidOperationException("Item container extension cannot be created when the item article container information is null.");
            }

            ItemReference = item;
        }

        internal Item ItemReference
        {
            get;
            private init;
        }

        public IReadOnlyCollection<Item> ContainedItens => containedItens;

        internal readonly HashSet<Item> containedItens = [];

        public Mass? ContainedMass
        {
            get
            {
                Mass? containedMass = Mass.Zero;

                foreach(var item in ContainedItens)
                {
                    if (item.Article.Mass is null)
                    {
                        return null;
                    }

                    containedMass += item.Article.Mass;
                }

                return containedMass;
            }
        }

        public Mass? MassAvailableCapacity
            => ItemReference.Article.ContainerInformation?.MassCapacity - ContainedMass;

        public Volume? ContainedVolume
        {
            get
            {
                Volume? containedVolume = Volume.Zero;

                foreach (var item in ContainedItens)
                {
                    if (item.Article.Volume is null)
                    {
                        return null;
                    }

                    containedVolume += item.Article.Volume;
                }

                return containedVolume;
            }
        }

        public Volume? VolumeAvailableCapacity
            => ItemReference.Article.ContainerInformation?.VolumeCapacity - ContainedVolume;

        public int? ItensQuantityAvailableCapacity
            => ItemReference.Article.ContainerInformation?.ItensQuantityCapacity - ContainedItens.Count;

        public Temperature? Temperature
        {
            get
            {
                if (TemperatureDefault is false)
                {
                    return field;
                }

                return ItemReference.Article.ContainerInformation?.DefaultTemperature;
            }
            set
            {
                field = value;

                TemperatureDefault = false;
            }
        }

        public bool TemperatureDefault
        { 
            get;
            set;
        } = true;

        public bool IsLocked
        {
            get;
            private set;
        } = false;

        public Description? LockReason
        {
            get;
            private set;
        }

        public void Lock(Description? reason = null)
        {
            IsLocked = true;
            LockReason = reason;
        }

        public void Unlock()
        {
            IsLocked = false;
            LockReason = null;
        }

        public void ValidateAdd(Item item)
        {
            if (item.ParentContainer == this.ItemReference)
            {
                return;
            }

            if (IsLocked)
            {
                throw new InvalidOperationException("Cannot insert item when container is locked.");
            }

            if (VolumeAvailableCapacity is not null && item.Article.Volume is null)
            {
                throw new ArgumentException("Cannot insert item with no volume information inside a contianer with volume capacity.", nameof(item));
            }            

            if (MassAvailableCapacity is not null && item.Article.Mass is null)
            {
                throw new ArgumentException("Cannot insert item with no mass information inside a container with mass capacity.", nameof(item));
            }

            if (VolumeAvailableCapacity < item.Article.Volume)
            {
                throw new InvalidOperationException("Cannot insert this item when the available volume capacity is lower than the item volume.");
            }

            if (MassAvailableCapacity < item.Article.Mass)
            {
                throw new InvalidOperationException("Cannot insert this item when the available mass capacity is lower than the item mass.");
            }

            if (ItensQuantityAvailableCapacity < 1)
            {
                throw new InvalidOperationException("Cannot insert this item when the available itens quantity capacity is lower than 0.");
            }
        }

        public void ValidateRemove(Item item)
        {
            if (item.ParentContainer != this.ItemReference)
            {
                throw new ArgumentException("Item parent container is not this.", nameof(item));
            }

            if (IsLocked)
            {
                throw new InvalidOperationException("Cannot remove item when container is locked.");
            }
        }
    }
}
