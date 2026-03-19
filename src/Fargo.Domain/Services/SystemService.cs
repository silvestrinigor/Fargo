namespace Fargo.Domain.Services;

/// <summary>
/// Provides domain operations and definitions related to the internal system.
/// </summary>
/// <remarks>
/// This service centralizes domain-level rules and constants associated with
/// system-initiated operations.
///
/// The internal system identity is used whenever an operation is performed
/// by the application itself rather than by a real authenticated user.
///
/// Examples include:
/// - background workers
/// - automatic processes
/// - database seeding
/// - migrations
/// </remarks>
public class SystemService
{
    private const string systemGuidString = "00000000-0000-0000-0000-000000000001";

    /// <summary>
    /// Gets the default unique identifier representing the internal system actor.
    /// </summary>
    /// <remarks>
    /// This identifier is constant across the domain and must be used whenever
    /// an action is performed by the system itself.
    /// </remarks>
    public static Guid SystemGuid { get; } = new(systemGuidString);
}
