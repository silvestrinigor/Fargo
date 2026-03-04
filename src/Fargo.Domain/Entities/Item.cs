namespace Fargo.Domain.Entities
{
    public class Item : Entity
    {
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
    }
}