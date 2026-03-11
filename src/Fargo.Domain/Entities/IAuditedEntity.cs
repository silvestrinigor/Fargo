namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Defines the auditing contract for entities that track creation
    /// and last modification metadata.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface expose information about when
    /// the entity was created, which user created it, when it was last
    /// edited, and which user performed the last modification.
    ///
    /// The auditing values are typically assigned by the application
    /// or infrastructure layer during persistence operations.
    /// </remarks>
    public interface IAuditedEntity
    {
        /// <summary>
        /// Gets the date and time when the entity was created.
        /// </summary>
        DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets the unique identifier of the user that created the entity.
        /// </summary>
        /// <remarks>
        /// This value may be <see langword="null"/> when the entity was created
        /// by a system process or when the user context is not available.
        /// </remarks>
        Guid? CreatedByGuid { get; }

        /// <summary>
        /// Gets the date and time when the entity was last modified.
        /// </summary>
        /// <remarks>
        /// This value is <see langword="null"/> when the entity has not
        /// been modified since its creation.
        /// </remarks>
        DateTimeOffset? EditedAt { get; }

        /// <summary>
        /// Gets the unique identifier of the user that last modified the entity.
        /// </summary>
        /// <remarks>
        /// This value is <see langword="null"/> when the entity has not
        /// been modified since its creation.
        /// </remarks>
        Guid? EditedByGuid { get; }

        /// <summary>
        /// Marks the entity as edited by the specified user.
        /// </summary>
        /// <param name="userGuid">
        /// The unique identifier of the user performing the modification.
        /// </param>
        /// <remarks>
        /// This method updates the modification audit metadata of the entity.
        /// Implementations are expected to set the last edition timestamp
        /// and the identifier of the user responsible for the change.
        /// </remarks>
        void MarkAsEdited(Guid userGuid);
    }
}