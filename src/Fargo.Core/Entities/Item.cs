namespace Fargo.Domain.Entities
{
    public class Item
    {
        internal Item() { }

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
                if (value is not null && !value.Article.IsContainer)
                    throw new InvalidOperationException(
                        "Cannot set parent item when the value article is not a container.");

                ParentItemGuid = value?.Guid;
                field = value;
            }
        }
    }
}
