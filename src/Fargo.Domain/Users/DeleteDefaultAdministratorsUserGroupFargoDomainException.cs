namespace Fargo.Domain.Exceptions;

/// <summary>
/// Exception thrown when an attempt is made to delete the default administrators user group.
/// </summary>
/// <remarks>
/// The default administrators user group is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteDefaultAdministratorsUserGroupFargoDomainException()
    : FargoDomainException("The default administrators user group cannot be deleted.")
{
}
