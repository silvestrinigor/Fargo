using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities.Models
{
    public class User : Model
    {
        internal User() : base() { }

        public override ModelType ModelType => ModelType.User;

        public required Name FirstName { get; init; }

        public required Name LastName { get; init; }

        public Description Description { get; init; } = Description.Empty;

        public string? Password
        {
            get; init;
        }
    }
}
