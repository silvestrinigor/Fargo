namespace Fargo.Domain.ValueObjects.Entities
{
    /// <summary>
    /// Represents a lightweight information projection of a user.
    /// </summary>
    /// <remarks>
    /// This value object contains essential information about a user without loading
    /// the full <c>User</c> aggregate. It is typically used in queries, listings,
    /// authentication contexts, and authorization checks where only user metadata
    /// and access information are required.
    /// </remarks>
    /// <param name="Guid">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="Nameid">
    /// The unique name identifier used to reference the user in the system.
    /// </param>
    /// <param name="FirstName">
    /// The user's first name, if provided.
    /// </param>
    /// <param name="LastName">
    /// The user's last name, if provided.
    /// </param>
    /// <param name="Description">
    /// A description containing additional information about the user.
    /// </param>
    /// <param name="DefaultPasswordExpirationPeriod">
    /// The default period after which the user's password expires.
    /// </param>
    /// <param name="RequirePasswordChangeAt">
    /// The date and time at which the user is required to change their password.
    /// </param>
    /// <param name="IsActive">
    /// Indicates whether the user account is currently active.
    /// </param>
    /// <param name="Permissions">
    /// The collection of permissions directly assigned to the user.
    /// </param>
    /// <param name="PartitionAccesses">
    /// The collection of partition identifiers that the user is allowed to access.
    /// </param>
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