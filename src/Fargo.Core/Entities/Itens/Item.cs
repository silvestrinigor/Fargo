using Fargo.Domain.Entities.Articles;
using Fargo.Domain.Exceptions.Entities.Itens;

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

        public Article Article
        { 
            get;
            private init;
        }

        public ItemContainerExtension? ContainerExtension
        {
            get;
            private init => field
                = value is not null && value.Item != this
                ? throw new InvalidOperationException("Container extension item should be this one.")
                : value;
        }

        public Item? ParentContainer
        {
            get;
            internal set
            {
                field
                = value == this
                ? throw new ItemParentEqualsItemException()
                : !value?.Article.IsContainer ?? false
                ? throw new ItemIsNotContainerException()
                : field is not null && value is not null && value != field.ParentContainer && value.ParentContainer != field.ParentContainer
                ? throw new ContainerOutOfItemRangeException()
                : value;

                InsertedInContainerAt = DateTime.UtcNow;
            }
        }

        public DateTime? InsertedInContainerAt { get; private set; }

        public bool IsInContainer => ParentContainer is not null;
    }
}
