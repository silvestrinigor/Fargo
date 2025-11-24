using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    public class Container : NamedEntity
    {
        public event EventHandler? EntityAdded;
        public event EventHandler? EntityRemoved;

        private void OnArticleItemAdded()
        {
            EntityAdded?.Invoke(this, EventArgs.Empty);
        }

        private void OnArticleItemRemoved()
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
            OnArticleItemAdded();
        }

        public void Remove(ArticleItem item)
        {
            if (item.Container == this) item.Container = null;
            OnArticleItemRemoved();
        }
    }
}