namespace Fargo.Application.Identity;

/// <summary>
/// Exception thrown when the current user is not authorized
/// to perform the requested operation.
/// </summary>
public class UnauthorizedAccessException()
    : Exception("The current actor is not authorized to perform this operation.");
