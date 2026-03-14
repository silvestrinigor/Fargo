namespace Fargo.Domain.ValueObjects.Entities
{
    public sealed record UserGroupInformation(
            Guid Guid,
            Nameid Nameid,
            Description Description,
            bool IsActive,
            IReadOnlyCollection<Permission> Permissions
            );
}