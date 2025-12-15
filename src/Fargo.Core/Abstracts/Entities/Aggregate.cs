using Fargo.Domain.Interfaces.Events;

namespace Fargo.Domain.Abstracts.Entities
{
    public abstract class Aggregate : Entity
    {
        public IReadOnlyCollection<IEntityEvent> UnsavedEvents => unsavedEvents;

        private readonly List<IEntityEvent> unsavedEvents = [];

        protected void RaiseEvent(IEntityEvent entityEvent)
        {
            Apply(entityEvent);
            unsavedEvents.Add(entityEvent);
        }

        public void ClearEvents() => unsavedEvents.Clear();

        protected abstract void Apply(IEntityEvent entityEvent);

        public void LoadFromHistory(IEnumerable<IEntityEvent> history)
        {
            foreach (var e in history)
            {
                Apply(e);
            }
        }
    }
}
