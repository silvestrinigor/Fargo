using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    public class Container : Entity
    {
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
            if (entity.Parent == this.Guid) return;
            if (entity == this) throw new InvalidOperationException("Cannot add container to itself.");
            if (entity.Parent != this.Parent) throw new InvalidOperationException("Entity is in a different container.");
            entity.Parent = this.Guid;
            OnItemAdded();
        }

        public virtual void Remove(Entity entity)
        {
            if (entity.Parent != this.Guid) throw new InvalidOperationException("Entity is in a child container or outside of this container.");
            entity.Parent = this.Parent;
            OnItemRemoved();
        }
    }
}