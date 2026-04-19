namespace Fargo.Application.Authentication;

/// <summary>
/// Exception thrown when a nameid string does not satisfy the required format rules.
/// </summary>
/// <param name="reason">A message describing the specific rule violation.</param>
public sealed class InvalidNameidFargoApplicationException(string reason)
    : FargoApplicationException(reason);
