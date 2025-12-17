using Fargo.Domain.Abstracts.Entities;
using Fargo.Domain.ValueObjects.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public class Article : Entity
    {
        public Name? Name { get; set; }

        public Description? Description { get; set; }

        public Temperature? TemperatureMax { get; init; }

        public Temperature? TemperatureMin { get; init; }

        public UnitArticle? Item
        {
            get;
            init
            {
                field = (value is null || value.Article != this)
                    ? value : throw new InvalidOperationException("ItemArticle's Article reference must point back to this Article.");
            }
        }

        public ContainerArticle? Container
        {
            get;
            init
            {
                field = (value is null || value.Article != this) 
                    ? value : throw new InvalidOperationException("ContainerArticle's Article reference must point back to this Article.");
            }
        }

        public bool IsItem => Item is not null;

        public bool IsContainer => Container is not null;
    }
}
