using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    // Article item probaly should not be a NamedEntity, 
    // if user wants to give a name, he puts the article item in a article bunch or someting
    public class ArticleItem : NamedEntityOptional // TODO:? Change the class name to ArticleUnit?
    {
        public required Article Article { get; init; }
        public Container? Container { get; internal set; }
        public DateTime? ShelfDate => Article.ShelfLife is not null
            ? CreatedAt + Article.ShelfLife.Value
            : null;
    }
}