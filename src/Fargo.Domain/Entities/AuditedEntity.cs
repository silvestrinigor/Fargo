namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Base class for entities that track who and when performed the last modification.
    /// </summary>
    public abstract class AuditedEntity : Entity
    {
        public DateTimeOffset UpdatedAt
        {
            get;
            set;
        } = DateTimeOffset.UtcNow;

        public Guid? UpdatedByUserGuid
        {
            get;
            private set;
        }

        public required User? UpdatedBy
        {
            get;
            set
            {
                UpdatedByUserGuid = value?.Guid;
                field = value;
            }
        }
    }
}