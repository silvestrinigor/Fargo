namespace Fargo.Domain.Security
{
    /// <summary>
    /// Represents the internal system actor.
    /// </summary>
    /// <remarks>
    /// This actor is used when an operation is performed by the system
    /// itself rather than a real authenticated user.
    ///
    /// Examples:
    /// - background workers
    /// - automatic processes
    /// - database seeding
    /// - migrations
    /// </remarks>
    public static class SystemActor
    {
        private const string systemActorGuidString = "00000000-0000-0000-0000-000000000001";

        /// <summary>
        /// The predefined unique identifier representing the system actor.
        /// </summary>
        public static readonly Guid Guid = new(systemActorGuidString);
    }
}