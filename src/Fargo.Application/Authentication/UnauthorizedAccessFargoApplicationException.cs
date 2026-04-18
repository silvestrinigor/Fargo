namespace Fargo.Application.Authentication;

/// <summary>
/// Exception thrown when the current user is not authorized
/// to perform the requested operation.
/// </summary>
public class UnauthorizedAccessFargoApplicationException()
    : FargoApplicationException(
            "The current user is not authorized to perform this operation.");
