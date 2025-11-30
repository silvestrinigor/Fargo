using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    public class Item : Entity
    {
        public required Article Article { get; init; }
        public DateTime? ShelfDate => Article?.ShelfLife != null
            ? CreatedAt + Article.ShelfLife.Value
            : null;
    }
}