using Fargo.Domain.Abstracts.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class Container : NamedEntityOptional
    {
        public Place? Place { get; internal set; }

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

        public void Add(ArticleItem item)
        {
            /*
             * WHY: older container remove(item)
             * to generate event of entity removed
             * and in future can implement a validation for remove a item from a container
             */
            item.Container?.Remove(item);
            item.Container = this;
            OnEntityAdded();
        }

        public void Remove(ArticleItem item)
        {
            if (item.Container == this) item.Container = null;
            OnEntityRemoved();
        }
    }
}