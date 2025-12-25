using Fargo.Domain.Entities.Articles;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    public class ArticleService(IArticleRepository articleRepository)
    {
        private readonly IArticleRepository articleRepository = articleRepository;

        public Article CreateArticle(Article article)
        {
            articleRepository.Add(article);
            OnArticleCreated();
            return article;
        }

        public event EventHandler? ArticleCreated;

        private void OnArticleCreated()
        {
            ArticleCreated?.Invoke(this, EventArgs.Empty);
        }
    }
}
