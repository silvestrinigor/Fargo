using Fargo.Domain.ValueObjects.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities.Itens
{
    public class ItemContainerExtension
    {
        public ItemContainerExtension(Item item)
        {
            if (!item.Article.IsContainer)
                throw new InvalidOperationException("Item is not a container.");

            MassAvailableCapacity = item.Article.Container?.MassCapacity;
            VolumeAvailableCapacity = item.Article.Container?.VolumeCapacity;
            this.item = item;
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

        private readonly Item item;

        public void Lock(Description? reason = null)
        {
            IsLocked = true;
            LockReason = reason;
            OnLocked();
        }

        public void Unlock()
        { 
            IsLocked = false; 
            LockReason = null;
            OnUnlocked();
        }

        public void Insert(Item item)
        {
            if (item.Container == this.item)
                throw new InvalidOperationException("Item is already inside the container.");

            if (IsLocked)
                throw new InvalidOperationException("Itens can't be inserted inside a locked container.");

            if (VolumeAvailableCapacity < item.Article.Volume)
                throw new InvalidOperationException("Container don't have volume capacity to add this item.");

            if (MassAvailableCapacity < item.Article.Mass)
                throw new InvalidOperationException("Container don't have mass capacity to add this item.");

            item.Container = this.item;

            MassAvailableCapacity -= item.Article.Mass;
            VolumeAvailableCapacity -= item.Article.Volume;

            OnItemInserted();
        }

        public void Remove(Item item)
        {
            if (item.Container != this.item)
                throw new InvalidOperationException("Item is not inside this container.");

            if (IsLocked)
                throw new InvalidOperationException("Itens can't be removed from a locked container.");

            item.Container = this.item.Container;

            MassAvailableCapacity += item.Article.Mass;
            VolumeAvailableCapacity += item.Article.Volume;

            OnItemRemoved();
        }

        public event EventHandler? ItemInserted;

        protected virtual void OnItemInserted()
        {
            ItemInserted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? ItemRemoved;

        protected virtual void OnItemRemoved()
        {
            ItemRemoved?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? Locked;

        protected virtual void OnLocked()
        {
            Locked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? Unlocked;

        protected virtual void OnUnlocked()
        {
            Unlocked?.Invoke(this, EventArgs.Empty);
        }
    }
}
