namespace Fargo.Domain.Entities
{
    public class Item
    {
        public Guid Guid { get; init; } = Guid.NewGuid();

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public Guid ArticleGuid { get; private init; }

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
