namespace Fargo.Domain.ValueObjects.Entities
{
    public sealed record UserInformation(
            Guid Guid,
            Nameid Nameid,
            FirstName? FirstName,
            LastName? LastName,
            Description Description,
            TimeSpan DefaultPasswordExpirationPeriod,
            DateTimeOffset RequirePasswordChangeAt,
            bool IsActive,
            IReadOnlyCollection<Permission> Permissions,
            IReadOnlyCollection<Guid> PartitionAccesses
            );
}