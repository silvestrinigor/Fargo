using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Entities.Articles
{
    public class PhysicalProductArticle
    {
        public Name? Name { get; set; }

        public Description? Description { get; set; }

        public PhysicalProductArticleGeometry Geometry { get; }

        public PhysicalProductArticleHeaviness Heaviness { get; }

    }
}
