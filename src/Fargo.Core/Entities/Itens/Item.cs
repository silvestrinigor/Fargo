using Fargo.Domain.Entities.Articles;
using Fargo.Domain.Exceptions.Entities.Itens;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Entities.Itens
{
    public class Item : Entity
    {
        internal Item(Article article, Name? name = null, Description? description = null) : base(name, description)
        {
            Article = article;

            ContainerExtension
                = article.IsContainer
                ? new ItemContainerExtension(this)
                : null;
        }

        public Item(IItemService service, Article article, Name? name = null, Description? description = null) : this(article, name, description)
        {
            service.CreateNewEntity(this);
        }

        public Article Article
        { 
            get;
            private init;
        }

        public DateTime? ManufacturedAt
        {
            get;
            init => field 
                = value > DateTime.Now
                ? throw new InvalidOperationException()
                : value;

        } = DateTime.UtcNow;

        public DateTime? ExpirationDate
            => ManufacturedAt + Article.ShelfLife;

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
            }
        }

        public bool IsInContainer => ParentContainer is not null;
    }
}
