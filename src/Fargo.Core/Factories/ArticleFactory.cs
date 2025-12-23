using Fargo.Domain.Entities.Articles;
using UnitsNet;

namespace Fargo.Domain.Factories
{
    public class ArticleFactory
    {
        public event EventHandler? ArticleCreated;

        public PhysicalProductArticle CreateItemArticle()
        {
            var article = new PhysicalProductArticle
            {
                IsContainer = false
            };

            OnArticleCreated();

            return article;
        }

        public PhysicalProductArticle CreateContainerArticle(Mass? massCapacity, Volume? volumeCapacity, Temperature? temperature)
        {
            var article = new PhysicalProductArticle
            {
                IsContainer = true,
                ContainerMassCapacity = massCapacity,
                ContainerVolumeCapacity = volumeCapacity,
                ContainerTemperature = temperature
            };

            OnArticleCreated();

            return article;
        }

        private void OnArticleCreated()
        {
            ArticleCreated?.Invoke(this, EventArgs.Empty);
        }
    }
}
