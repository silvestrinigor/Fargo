using Fargo.Domain.Exceptions.Entities.Itens;
using Fargo.Domain.ValueObjects.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities.Itens
{
    public class ItemContainerExtension
    {
        public ItemContainerExtension(Item item)
        {
            if (item.Article.ContainerInformation is null)
                throw new ItemIsNotContainerException();

            MassAvailableCapacity = item.Article.ContainerInformation?.MassCapacity;
            VolumeAvailableCapacity = item.Article.ContainerInformation?.VolumeCapacity;
            ItensQuantityAvailableCapacity = item.Article.ContainerInformation?.ItensQuantityCapacity;
            Item = item;
        }

        public Mass? MassAvailableCapacity 
        { 
            get;
            private set;
        }

        public Volume? VolumeAvailableCapacity 
        { 
            get; 
            private set;
        }

        public int? ItensQuantityAvailableCapacity
        {
            get; 
            private set; 
        }

        public Temperature? Temperature
        { 
            get => field is not null
                ? field
                : Item.Article.ContainerInformation?.DefaultTemperature;
            init;
        }

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

        public Item Item
        {
            get;
            private init => field
                = !value.Article.IsContainer
                ? throw new ItemIsNotContainerException()
                : value;
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

        public void Insert(Item item)
        {
            if (item.ParentContainer == this.Item)
                throw new ItemAlreadyInsideContainerException();

            if (IsLocked)
                throw new ContainerLockedException();

            if (VolumeAvailableCapacity is not null && item.Article.Volume is null)
                throw new LackOfInformationException();

            if (MassAvailableCapacity is not null && item.Article.Mass is null)
                throw new LackOfInformationException();

            if (VolumeAvailableCapacity < item.Article.Volume)
                throw new InsufficientAvailableCapacityException();

            if (MassAvailableCapacity < item.Article.Mass)
                throw new InsufficientAvailableCapacityException();

            if (ItensQuantityAvailableCapacity < 1)
                throw new InsufficientAvailableCapacityException();

            item.ParentContainer = this.Item;

            MassAvailableCapacity -= item.Article.Mass;
            VolumeAvailableCapacity -= item.Article.Volume;
            ItensQuantityAvailableCapacity--;
        }

        public void Remove(Item item)
        {
            if (item.ParentContainer != this.Item)
                throw new ContainerOutOfItemRangeException();

            if (IsLocked)
                throw new ContainerLockedException();

            item.ParentContainer = this.Item.ParentContainer;

            MassAvailableCapacity += item.Article.Mass;
            VolumeAvailableCapacity += item.Article.Volume;
            ItensQuantityAvailableCapacity++;
        }
    }
}
