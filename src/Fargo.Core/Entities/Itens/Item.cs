using Fargo.Domain.Entities.Articles;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Entities.Itens
{
    public class Item
    {
        public Name? Name 
        { 
            get; 
            set; 
        }

        public Description? Description 
        { 
            get; 
            set; 
        }

        public Article Article { get; }

        public ItemContainerExtension? ContainerExtension {  get; }

        public Item? Container
        {
            get;
            private set => field =
                value == this
                ? throw new InvalidOperationException("Cannot place item into itself.")
                :
                value is not null && !value.Article.IsContainer
                ? throw new InvalidOperationException("The specified container item is not a container.")
                :
                value is null && field is not null && field.Container is not null
                ? throw new InvalidOperationException()
                : 
                value is not null && field is not null && field.Container != value && value.Container != field.Container
                ? throw new InvalidOperationException()
                : value;
        }

        public bool IsInContainer => Container is not null;
        
        public Item(Article article)
        {
            Article = article;

            ContainerExtension = article.IsContainer ? new ItemContainerExtension(this) : null;
        }

        public void InsertIntoContainer(Item item)
        {
            Container =
                item.ContainerExtension?.VolumeAvailableCapacity < this.Article.Volume
                ? throw new InvalidOperationException()
                :
                item.ContainerExtension?.MassAvailableCapacity < this.Article.Mass
                ? throw new InvalidOperationException()
                : item;
        }

        public void RemoveFromContainer()
        {
            Container = 
                IsInContainer
                ? throw new InvalidOperationException("Item is not in a container.")
                : Container?.Container;
        }
    }
}
