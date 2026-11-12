namespace Fargo.RealtimeContracts;

/// <summary>
/// Tells a client that an entity changed. It carries no entity state: the
/// client reads the entity again through the API, which applies the actor's
/// permissions and partition access to that read.
/// </summary>
/// <param name="EntityType">The kind of entity, one of <see cref="EntityTypes"/>.</param>
/// <param name="ChangeType">One of <see cref="ChangeTypes"/>.</param>
/// <param name="EventId">The event identifier; delivery is at-least-once, so a client may see it twice.</param>
/// <param name="OccurredAt">The instant, in UTC, at which the change was saved.</param>
public sealed record EntityChangedNotification(
    string EntityType,
    Guid Guid,
    string ChangeType,
    Guid EventId,
    DateTimeOffset OccurredAt)
{
    /// <summary>
    /// The values of <see cref="EntityType"/>.
    /// </summary>
    public static class EntityTypes
    {
        public const string Article = "article";
    }

    /// <summary>
    /// The values of <see cref="ChangeType"/>.
    /// </summary>
    public static class ChangeTypes
    {
        public const string Created = "created";

        /// <summary>
        /// The entity, or one of its parts, was edited.
        /// </summary>
        public const string Edited = "edited";

        public const string Deleted = "deleted";
    }
}
