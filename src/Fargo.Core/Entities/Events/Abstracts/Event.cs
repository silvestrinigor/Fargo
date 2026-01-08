using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events.Abstracts
{
    public abstract class Event : IEntity
    {
        internal Event() { }

        internal Event(Model model)
        {
            ModelGuid = model.Guid;
            ModelType = model.ModelType;
        }

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public abstract EventType EventType
        {
            get;
            protected init;
        }

        public DateTime OccurredAt
        {
            get;
            init;
        } = DateTime.UtcNow;

        public Guid ModelGuid
        {
            get;
            private init;
        }

        public ModelType ModelType
        {
            get;
            private init;
        }
    }
}
