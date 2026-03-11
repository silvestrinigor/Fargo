namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a base entity that includes auditing information.
    /// </summary>
    /// <remarks>
    /// This entity extends <see cref="Entity"/> and implements
    /// <see cref="IAuditedEntity"/> to provide metadata used to track
    /// creation and modification events.
    ///
    /// The auditing metadata records:
    /// <list type="bullet">
    /// <item>
    /// <description>When the entity was created.</description>
    /// </item>
    /// <item>
    /// <description>Which user created the entity.</description>
    /// </item>
    /// <item>
    /// <description>When the entity was last modified.</description>
    /// </item>
    /// <item>
    /// <description>Which user performed the last modification.</description>
    /// </item>
    /// </list>
    ///
    /// These values are typically managed by the application or
    /// infrastructure layer during persistence operations.
    /// </remarks>
    public abstract class AuditedEntity : Entity, IAuditedEntity
    {
        /// <summary>
        /// Gets the date and time when the entity was created.
        /// </summary>
        /// <remarks>
        /// The value is initialized with <see cref="DateTimeOffset.UtcNow"/>
        /// when the entity instance is created.
        /// </remarks>
        public DateTimeOffset CreatedAt
        {
            get;
            init;
        } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets the unique identifier of the user that created the entity.
        /// </summary>
        /// <remarks>
        /// This value may be <see langword="null"/> when the entity was created
        /// by a system process or when the user context is not available.
        /// When provided, it identifies the user responsible for the creation.
        /// </remarks>
        public Guid? CreatedByGuid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the date and time when the entity was last modified.
        /// </summary>
        /// <remarks>
        /// This value is <see langword="null"/> when the entity has not been
        /// modified since its creation.
        /// </remarks>
        public DateTimeOffset? EditedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the unique identifier of the user that last modified the entity.
        /// </summary>
        /// <remarks>
        /// This value is <see langword="null"/> when the entity has not been
        /// modified since its creation.
        /// </remarks>
        public Guid? EditedByGuid
        {
            get;
            private set;
        }

        /// <summary>
        /// Marks the entity as edited by the specified user.
        /// </summary>
        /// <param name="userGuid">
        /// The unique identifier of the user performing the modification.
        /// </param>
        /// <remarks>
        /// This method updates the auditing metadata of the entity by
        /// assigning the current UTC time to <see cref="EditedAt"/> and
        /// storing the provided user identifier in <see cref="EditedByGuid"/>.
        ///
        /// It should be invoked whenever a modification to the entity occurs.
        /// </remarks>
        public void MarkAsEdited(Guid userGuid)
        {
            EditedAt = DateTimeOffset.UtcNow;
            EditedByGuid = userGuid;
        }
    }
}