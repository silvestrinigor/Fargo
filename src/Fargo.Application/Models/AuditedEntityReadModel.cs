namespace Fargo.Application.Models
{
    /// <summary>
    /// Represents a base read model that includes auditing metadata.
    /// </summary>
    /// <remarks>
    /// This model is used in the query side of the application (CQRS) to expose
    /// auditing information associated with persisted entities.
    ///
    /// It contains metadata describing when the record was created or modified
    /// and which user performed those operations.
    /// </remarks>
    public abstract class AuditedEntityReadModel
    {
        /// <summary>
        /// Gets the unique identifier of the entity.
        /// </summary>
        public required Guid Guid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the date and time when the entity was created.
        /// </summary>
        public required DateTimeOffset CreatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the unique identifier of the user that created the entity.
        /// </summary>
        /// <remarks>
        /// This value may be <c>null</c> when the record was created by a
        /// system process or when the creator information is not available.
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
        /// This value may be <c>null</c> when the entity has not been modified
        /// since its creation.
        /// </remarks>
        public DateTimeOffset? EditedAt
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the unique identifier of the user that last modified the entity.
        /// </summary>
        /// <remarks>
        /// This value may be <c>null</c> when the entity has not been modified
        /// since its creation.
        /// </remarks>
        public Guid? EditedByGuid
        {
            get;
            init;
        }
    }
}