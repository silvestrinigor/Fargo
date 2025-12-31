using Fargo.Domain.ValueObjects;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents an item with associated article information, optional descriptive metadata, and container
    /// capabilities. Supports tracking of manufacturing and expiration dates, as well as hierarchical containment
    /// within other items.
    /// </summary>
    /// <remarks>An item may represent a physical or logical entity, such as a product or a container. If the
    /// associated article is a container, the item can hold other items using its container extension. Items can be
    /// inserted into or removed from container items, supporting nested containment scenarios. Thread safety is not
    /// guaranteed; callers should ensure appropriate synchronization if accessing instances from multiple
    /// threads.</remarks>
    public class Item : Entity
    {
        /// <summary>
        /// Initializes a new instance of the Item class with the specified article, name, and description.
        /// </summary>
        /// <param name="article">The article that defines the characteristics and behavior of the item. Cannot be null.</param>
        public Item(Article article, Name? name = null, Description? description = null) : base(name, description)
        {
            Article = article;

            ContainerExtension
                = article.IsContainer
                ? new ItemContainer(this)
                : null;
        }

        /// <summary>
        /// Gets the article associated with this instance.
        /// </summary>
        public Article Article
        { 
            get;
            private init;
        }

        /// <summary>
        /// Gets a value indicating whether the current item is a container that can hold other items.
        /// </summary>
        public bool IsContainer => Article.IsContainer;

        /// <summary>
        /// Gets the date and time when the item was manufactured.
        /// </summary>
        /// <remarks>The value must not be set to a future date and time. By default, this property is
        /// initialized to the current UTC date and time. This property can only be set during object
        /// initialization.</remarks>
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
        
        /// <summary>
        /// Gets the date and time when the article expires, if available.
        /// </summary>
        /// <remarks>The expiration date is calculated by adding the article's shelf life to its
        /// manufacturing date. If either the manufacturing date or shelf life is not set, the expiration date will be
        /// null.</remarks>
        public DateTime? ExpirationDate
            => ManufacturedAt + Article.ShelfLife;

        /// <summary>
        /// Gets a value indicating whether the item has expired based on its expiration date.
        /// </summary>
        public bool IsExpired
            => ExpirationDate is not null && DateTime.UtcNow > ExpirationDate;

        /// <summary>
        /// Gets the container extension associated with this item, if the item represents a container; otherwise, null.
        /// </summary>
        /// <remarks>The value is only set when the item is a container. If the item is not a container,
        /// this property is always null. The container extension's ItemReference must refer to this item.</remarks>
        public ItemContainer? ContainerExtension
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

        /// <summary>
        /// Gets the parent container of this item, if any.
        /// </summary>
        /// <remarks>The parent container must be a valid container and cannot be the item itself. When
        /// setting this property, the new parent must either be within the current parent container or be the current
        /// grandparent container. This property is read-only outside the class.</remarks>
        public Item? ParentContainer
        {
            get;
            private set
            {
                if (value == this)
                {
                    throw new ArgumentException("Parent container cannot be equals the child.", nameof(ParentContainer));
                }

                if (value is not null && !value.IsContainer)
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

        /// <summary>
        /// Gets a value indicating whether this object is contained within a parent container.
        /// </summary>
        public bool IsInContainer => ParentContainer is not null;

        /// <summary>
        /// Inserts the current item into the specified container.
        /// </summary>
        /// <param name="container">The container into which this item will be inserted. Cannot be null and must not be locked.</param>
        /// <exception cref="InvalidOperationException">Thrown if the specified container is locked.</exception>
        public void InsertIntoContainer(Item container)
        {
            if (container.ContainerExtension!.IsLocked)
            {
                throw new InvalidOperationException("Cannot insert item when container is locked.");
            }

            ParentContainer = container;

            container?.ContainerExtension?.containedItens.Add(this);
        }

        /// <summary>
        /// Removes this item from its parent container, if it is currently contained within one.
        /// </summary>
        /// <remarks>If the item is not currently contained in a parent container, this method performs no
        /// action. After removal, the item's parent container reference is updated to the next parent in the hierarchy,
        /// if any.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the parent container is locked and items cannot be removed.</exception>
        public void RemoveFromContainer()
        {
            if (ParentContainer is null)
            {
                return;
            }

            if (ParentContainer.ContainerExtension!.IsLocked)
            {
                throw new InvalidOperationException("Cannot remove item when container is locked.");
            }

            ParentContainer.ContainerExtension?.containedItens.Remove(this);

            ParentContainer = ParentContainer?.ParentContainer;
        }
    }

    /// <summary>
    /// Represents a container item that can hold other items, providing access to its contents, capacity, and related
    /// container properties.
    /// </summary>
    /// <remarks>An ItemContainer exposes information and operations for items that are designated as
    /// containers. It allows querying the items contained, their total mass and volume, available capacity, and
    /// temperature settings. The container can be locked to prevent modifications, with an optional reason provided.
    /// Instances of this class are only valid for items whose article supports container functionality.</remarks>
    public class ItemContainer
    {
        /// <summary>
        /// Initializes a new instance of the ItemContainer class for the specified item, provided that the item
        /// represents a container.
        /// </summary>
        /// <param name="item">The item to associate with this container extension. The item must represent a container and have valid
        /// container information.</param>
        /// <exception cref="InvalidOperationException">Thrown if the specified item does not represent a container, or if its container information is null.</exception>
        internal ItemContainer(Item item)
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

        /// <summary>
        /// Gets the referenced item associated with this instance.
        /// </summary>
        internal Item ItemReference
        {
            get;
            private init;
        }

        /// <summary>
        /// Gets the collection of items contained within this instance.
        /// </summary>
        public IReadOnlyCollection<Item> ContainedItens => containedItens;

        internal readonly HashSet<Item> containedItens = [];

        /// <summary>
        /// Gets the total mass of all contained items, or null if any item does not have a defined mass.
        /// </summary>
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
    }
}
