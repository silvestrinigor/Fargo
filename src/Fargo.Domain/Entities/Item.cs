using Fargo.Domain.Exceptions;

namespace Fargo.Domain.Entities
{
    public class Item
    {
        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public Guid ArticleGuid
        {
            get;
            private init;
        }

        public required Article Article
        {
            get;
            init
            {
                ArticleGuid = value.Guid;

                field = value;
            }
        }

        public Guid? ParentItemGuid
        {
            get;
            private set;
        }

        public Item? ParentItem
        {
            get;
            internal set
            {
                if (value?.Article.IsContainer == false)
                    throw new ItemParentNotContainerException(value);

                if (value?.Guid == Guid)
                    throw new ItemParentEqualsItemException(value);

                ParentItemGuid = value?.Guid;

                field = value;
            }
        } = null;

        private readonly HashSet<Partition> partitions = [];

        public IReadOnlyCollection<Partition> Partitions => partitions;
    }
}