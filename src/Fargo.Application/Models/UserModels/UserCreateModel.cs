using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.Models.UserModels;

/// <summary>
/// Represents the data required to create a new user.
/// </summary>
/// <param name="Nameid">
/// The unique login identifier of the user.
/// This value must satisfy the validation rules defined in <see cref="Nameid"/>.
/// </param>
/// <param name="Password">
/// The plaintext password that will be securely hashed before being stored.
/// </param>
/// <param name="FirstName">
/// Optional first name of the user.
/// When provided, the value must satisfy the validation rules defined in
/// <see cref="FirstName"/>.
/// </param>
/// <param name="LastName">
/// Optional last name of the user.
/// When provided, the value must satisfy the validation rules defined in
/// <see cref="LastName"/>.
/// </param>
/// <param name="Description">
/// Optional textual description associated with the user.
/// If not provided, the default value defined by the domain will be used.
/// </param>
/// <param name="Permissions">
/// Optional list of permissions granted to the user.
/// Each permission defines an action the user is allowed to perform.
/// If not provided, the user will be created without additional permissions.
/// </param>
/// <param name="DefaultPasswordExpirationTimeSpan">
/// Optional password expiration interval configured for the user.
///
/// When the user successfully changes their password, this value is added
/// to the current UTC time to determine when the next password change
/// will be required.
///
/// If <see langword="null"/>, the system default expiration period defined
/// by the domain will be used.
///
/// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
/// immediately after it is changed.
/// </param>
/// <param name="FirstPartition">
/// Optional identifier of the first partition to associate with the user
/// during creation.
/// </param>
/// <remarks>
/// Users are partitioned entities and may belong to one or more partitions.
///
/// When <paramref name="FirstPartition"/> is provided, the user is initially
/// associated with that partition. Otherwise, the default partition behavior
/// defined by the application or domain is applied.
///
/// Security considerations:
/// <list type="bullet">
/// <item><description>The provided password is never stored in plaintext</description></item>
/// <item><description>Password hashing and validation rules are enforced by the domain</description></item>
/// <item><description>Permissions define the actions the user is authorized to perform</description></item>
/// </list>
/// </remarks>
public record UserCreateModel(
        string Nameid,
        string Password,
        FirstName? FirstName = null,
        LastName? LastName = null,
        Description? Description = null,
        IReadOnlyCollection<UserPermissionUpdateModel>? Permissions = null,
        TimeSpan? DefaultPasswordExpirationTimeSpan = null,
        Guid? FirstPartition = null
        );
