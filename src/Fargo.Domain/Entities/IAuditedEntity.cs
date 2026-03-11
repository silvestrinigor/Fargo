namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Defines the auditing contract for entities that track creation
    /// and last modification metadata.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface expose information about when
    /// the entity was created, which actor created it, when it was last
    /// edited, and which actor performed the last modification.
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
        /// Gets the unique identifier of the actor that created the entity.
        /// </summary>
        /// <remarks>
        /// This value may be <see langword="null"/> when the actor context
        /// is not available.
        ///
        /// When the entity is created by an internal system process,
        /// implementations should typically use
        /// <see cref="Security.SystemActor.Guid"/> rather than
        /// <see langword="null"/>.
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
        /// Gets the unique identifier of the actor that last modified the entity.
        /// </summary>
        /// <remarks>
        /// This value is <see langword="null"/> when the entity has not
        /// been modified since its creation.
        ///
        /// When the modification is performed by an internal system process,
        /// implementations should typically use
        /// <see cref="Security.SystemActor.Guid"/>.
        /// </remarks>
        Guid? EditedByGuid { get; }

        /// <summary>
        /// Marks the entity as created by the specified actor.
        /// </summary>
        /// <param name="userGuid">
        /// The unique identifier of the actor responsible for creating the entity.
        /// </param>
        /// <remarks>
        /// This method initializes the creation audit metadata of the entity
        /// by assigning the creation timestamp and the identifier of the actor
        /// responsible for the creation.
        ///
        /// When the creation is performed by the system, the caller should pass
        /// <see cref="Security.SystemActor.Guid"/>.
        /// </remarks>
        void MarkAsCreated(Guid? userGuid);

        /// <summary>
        /// Marks the entity as edited by the specified actor.
        /// </summary>
        /// <param name="userGuid">
        /// The unique identifier of the actor performing the modification.
        /// </param>
        /// <remarks>
        /// This method updates the modification audit metadata of the entity.
        /// Implementations are expected to set the last edition timestamp
        /// and the identifier of the actor responsible for the change.
        ///
        /// When the modification is performed by the system, the caller should pass
        /// <see cref="Security.SystemActor.Guid"/>.
        /// </remarks>
        void MarkAsEdited(Guid userGuid);
    }
}