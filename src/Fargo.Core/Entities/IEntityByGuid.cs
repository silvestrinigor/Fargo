namespace Fargo.Domain.Entities
{
    public interface IEntityByGuid : IEntity
    {
        Guid Guid { get; }
    }
}
