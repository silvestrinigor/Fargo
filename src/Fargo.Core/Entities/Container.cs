using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    public class Container : Entity
    {
        public Guid? ItemGuid { get; set; }

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

        public virtual void Add(Entity entity)
        {
            if (entity.ParentGuid == this.Guid) return;
            if (entity == this) throw new InvalidOperationException("Cannot add entity to itself.");
            if (entity.ParentGuid != this.ParentGuid) throw new InvalidOperationException("Entity is in a different parent or inside a child entity.");
            entity.ParentGuid = this.Guid;
            OnItemAdded();
        }

        public virtual void Remove(Entity entity)
        {
            if (entity.ParentGuid != this.Guid) throw new InvalidOperationException("Entity is in a child entity or outside of this container.");
            entity.ParentGuid = this.ParentGuid;
            OnItemRemoved();
        }
    }
}