using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    // a portion of space available or designated for or being used by someone.
    // works like container, but can't be moved?

    public class Place : NamedEntity
    {
        public event EventHandler? EntityAdded;
        public event EventHandler? EntityRemoved;

        private void OnEntityAdded()
        {
            EntityAdded?.Invoke(this, EventArgs.Empty);
        }

        private void OnEntityRemoved()
        {
            EntityRemoved?.Invoke(this, EventArgs.Empty);
        }

        public void Add(Container container)
        {
            container.Place?.Remove(container);
            container.Place = this;
            OnEntityAdded();
        }

        public void Remove(Container container)
        {
            if (container.Place == this) container.Place = null;
            OnEntityRemoved();
        }
    }
}
