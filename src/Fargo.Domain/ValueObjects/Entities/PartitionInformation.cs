namespace Fargo.Domain.ValueObjects.Entities
{
    public sealed record PartitionInformation(
        Guid Guid,
        Name Name,
        Description Description,
        Guid? ParentPartitionGuid,
        bool IsActive
    );
}