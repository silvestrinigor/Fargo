using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Entities
{
    public abstract class Entity : IEquatable<Entity>
    {
        internal Entity() { }

        internal Entity(Name? name, Description? description)
        {
            NameDescriptionInformation = new EntityNameDescriptionInformation
            {
                Name = name,
                Description = description
            };
        }

        public Guid Guid
        { 
            get; 
            init; 
        } = Guid.NewGuid();

        public DateTime CreatedAt 
        { 
            get; 
            init; 
        } = DateTime.UtcNow;

        public bool Equals(Entity? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Guid == other.Guid;
        }

        public override bool Equals(object? obj)
            => obj is Entity other && Equals(other);

        public override int GetHashCode()
            => Guid.GetHashCode();

        public static bool operator ==(Entity? left, Entity? right)
            => Equals(left, right);

        public static bool operator !=(Entity? left, Entity? right)
            => !Equals(left, right);

        public EntityNameDescriptionInformation? NameDescriptionInformation
        {
            get;
            protected init;
        }

        public bool HasNameDescriptionInformation
            => NameDescriptionInformation is not null;
    }
}
