using Fargo.Domain.Abstracts.Entities;
using Fargo.Domain.Entities.Articles;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Entities.Itens
{
    public partial class Item : Entity
    {
        public Name? Name { get; set; }
        
        public Description? Description { get; set; }

        public required Article Article
        {
            get;
            init
            {
                field = value.IsItem
                    ? value : throw new InvalidOperationException("Article must be a ItemArticle.");
            }
        }

        public Item? Container
        {
            get;
            private set
            {
                if (this.Article.TemperatureMin.HasValue)
                {
                    if (value?.Article.ContainerTemperature is null)
                        throw new InvalidOperationException("Cannot place item with defined minimum temperature into a container with undefined temperature.");

                    if (value.Article.ContainerTemperature < this.Article.TemperatureMin)
                        throw new InvalidOperationException("Container's temperature is below item's minimum temperature.");
                }

                if (this.Article.TemperatureMax.HasValue)
                {
                    if (value?.Article.ContainerTemperature is null)
                        throw new InvalidOperationException("Cannot place item with defined maximum temperature into a container with undefined temperature.");

                    if (value.Article.ContainerTemperature > this.Article.TemperatureMax)
                        throw new InvalidOperationException("Container's temperature exceeds item's maximum temperature.");
                }

                field = value;
            }
        }
    }
}