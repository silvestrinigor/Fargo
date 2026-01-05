using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public interface IEntity
    {
        Guid Guid { get; }
    }
}
