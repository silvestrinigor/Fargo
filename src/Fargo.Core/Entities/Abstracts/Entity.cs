using Fargo.Core.Enums;

namespace Fargo.Core.Entities.Abstracts
{
    public abstract class Entity : Base
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public required EEntityType EntityType { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.Now;
    }
}
