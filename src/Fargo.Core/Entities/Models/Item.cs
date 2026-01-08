namespace Fargo.Domain.Entities.Models
{
    public class Item : IEntity
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
    }
}
