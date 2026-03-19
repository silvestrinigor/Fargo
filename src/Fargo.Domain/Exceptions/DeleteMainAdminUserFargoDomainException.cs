namespace Fargo.Domain.Exceptions;

/// <summary>
/// Exception thrown when an attempt is made to delete the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteMainAdminUserFargoDomainException()
    : FargoDomainException("The main administrator user cannot be deleted.")
{
}
