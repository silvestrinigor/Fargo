namespace Fargo.Domain.Entities
{
    public class Item : IEntity, IEntityByGuid, IEntityTemporal
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
                    throw new InvalidOperationException(
                        "Cannot set parent item when the value article is not a container.");

                if (value?.Guid == Guid)    
                    throw new InvalidOperationException(
                        "An item cannot be moved into itself.");

                ParentItemGuid = value?.Guid;

                field = value;
            }
        } = null;
    }
}