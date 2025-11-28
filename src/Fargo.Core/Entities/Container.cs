namespace Fargo.Domain.Entities
{
    public class Container : Item
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

        public void Add(Item item)
        {
            if (item.Container == this) return;
            if (item == this) throw new InvalidOperationException("Cannot add container to itself.");
            if (item.Container != this.Container) throw new InvalidOperationException("Item is in a different container.");
            item.Container = this;
            OnItemAdded();
        }

        public void Remove(Item item)
        {
            if (item.Container != this) throw new InvalidOperationException("Item is in a child container or different container.");
            item.Container = this.Container;
            OnItemRemoved();
        }
    }
}