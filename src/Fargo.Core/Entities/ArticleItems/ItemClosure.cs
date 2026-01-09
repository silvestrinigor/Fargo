namespace Fargo.Domain.Entities.ArticleItems
{
    public class ItemClosure
    {
        internal ItemClosure() { }

        public Guid AncestorItemGuid 
        { 
            get; 
            private init;
        }

        public required Item AncestorItem
        { 
            get;
            init
            {
                AncestorItemGuid = value.Guid;
                field = value;
            }
        }

        public Guid DescendantItemGuid
        {
            get;
            private init;
        }

        public required Item DescendantItem
        {
            get;
            init
            {
                DescendantItemGuid = value.Guid;
                field = value;
            }
        }

        public required int Depth
        { 
            get; 
            set; 
        }
    }
}
