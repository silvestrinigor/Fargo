using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    public class ArticleItem : NamedEntity // TODO:? Change the class name to ArticleUnit?
    {
        public required Article Article { get; init; }
        public Container? Container { get; internal set; }
    }
}