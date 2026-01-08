using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities.Models
{
    public class User : IEntity
    {
        internal User() : base() { }

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public required Name FirstName { get; init; }

        public required Name LastName { get; init; }

        public Description Description { get; init; } = Description.Empty;

        public string? Password
        {
            get; init;
        }
    }
}
