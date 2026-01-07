using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Models.Abstracts
{
    public abstract class Model : IEntity
    {
        internal Model() { }

        public Guid Guid
        {
            get; 
            init;
        } = Guid.NewGuid();

        public abstract ModelType ModelType
        { 
            get;
        }
    }
}
