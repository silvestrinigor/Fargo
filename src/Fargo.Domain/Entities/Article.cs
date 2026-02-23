using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Article
    {
        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public Name Name
        {
            get;
            private set;
        }

        public void SetName(Name name, User updatedBy)
        {
            Name = name;
            UpdatedByUser = updatedBy;
        }

        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        public void SetDescription(Description description, User updatedBy)
        {
            Description = description;
            UpdatedByUser = updatedBy;
        }

        public bool IsContainer
        {
            get;
            init;
        } = false;

        public IReadOnlyCollection<Partition> Partitions => partitions;

        private readonly HashSet<Partition> partitions = [];

        public Guid UpdatedByUserGuid
        {
            get;
            private set;
        }

        public required User UpdatedByUser
        {
            get;
            set
            {
                UpdatedByUserGuid = value.Guid;
                field = value;
            }
        }
    }
}