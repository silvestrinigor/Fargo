using Fargo.Domain.Entities.Articles;

namespace Fargo.Domain.Entities.Itens
{
    public class Item : OptionalNamedEntity
    {
        public Item(Article article)
        {
            Article = article;

            ContainerExtension
                = article.IsContainer
                ? new ItemContainerExtension(this)
                : null;
        }

        public Article Article { get; }

        public ItemContainerExtension? ContainerExtension {  get; }

        public Item? Container
        {
            get;
            internal set => field
                = value == this
                ? throw new InvalidOperationException("Cannot place item into itself.")
                : !value?.Article.IsContainer ?? false
                ? throw new InvalidOperationException("The specified item is not a container.")
                : field is not null && value is not null && value != field.Container && value.Container != field.Container
                ? throw new InvalidOperationException(" ")
                : value;
        }

        public bool IsInContainer => Container is not null;
    }
}
