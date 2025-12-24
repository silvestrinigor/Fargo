using Fargo.Domain.Entities.Articles;
using UnitsNet;

namespace Fargo.Domain.Factories
{
    public class ArticleFactory
    {
        public event EventHandler? ArticleCreated;

        public Article CreateItemArticle()
        {
            var article = new Article
            {
                IsContainer = false
            };

            OnArticleCreated();

            return article;
        }

        public Article CreateContainerArticle(Mass? massCapacity, Volume? volumeCapacity, Temperature? temperature)
        {
            var article = new Article
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
