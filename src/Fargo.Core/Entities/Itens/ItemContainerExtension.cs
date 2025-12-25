using Fargo.Domain.Exceptions.Entities.Itens;
using Fargo.Domain.ValueObjects.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities.Itens
{
    public class ItemContainerExtension
    {
        public ItemContainerExtension(Item item)
        {
            MassAvailableCapacity = item.Article.Container?.MassCapacity;
            VolumeAvailableCapacity = item.Article.Container?.VolumeCapacity;
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

        public bool IsLocked 
        { 
            get; 
            private set; 
        }

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
            OnLocked();
        }

        public event EventHandler? Locked;

        protected virtual void OnLocked()
        {
            Locked?.Invoke(this, EventArgs.Empty);
        }

        public void Unlock()
        { 
            IsLocked = false; 
            LockReason = null;
            OnUnlocked();
        }

        public event EventHandler? Unlocked;

        protected virtual void OnUnlocked()
        {
            Unlocked?.Invoke(this, EventArgs.Empty);
        }

        public void Insert(Item item)
        {
            if (item.ParentContainer == this.Item)
                throw new ItemAlreadyInsideContainerException();

            if (IsLocked)
                throw new ContainerLockedException();

            if (VolumeAvailableCapacity < item.Article.Volume)
                throw new InsufficientAvailableCapacityException();

            if (MassAvailableCapacity < item.Article.Mass)
                throw new InsufficientAvailableCapacityException();

            item.ParentContainer = this.Item;

            MassAvailableCapacity -= item.Article.Mass;
            VolumeAvailableCapacity -= item.Article.Volume;

            OnItemInserted();
        }

        public event EventHandler? ItemInserted;

        protected virtual void OnItemInserted()
        {
            ItemInserted?.Invoke(this, EventArgs.Empty);
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

            OnItemRemoved();
        }

        public event EventHandler? ItemRemoved;

        protected virtual void OnItemRemoved()
        {
            ItemRemoved?.Invoke(this, EventArgs.Empty);
        }
    }
}
