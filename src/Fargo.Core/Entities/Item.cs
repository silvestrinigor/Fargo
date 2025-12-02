using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    public class Item : Entity
    {
        public required Guid ArticleGuid { get; init; }
    }
}