using Fargo.Domain.Entities.Events;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Events;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services
{
    public class ArticleService(IArticleRepository articleRepository, IEventRepository eventRepository)
    {
        private readonly IArticleRepository articleRepository = articleRepository;

        private readonly IEventRepository eventRepository = eventRepository;

        public async Task<Article?> GetArticleAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        {
            return await articleRepository.GetByGuidAsync(articleGuid, cancellationToken);
        }

        public Article CreateArticle(Name name, Description description)
        {
            var article = new Article
            {
                Name = name,
                Description = description
            };

            articleRepository.Add(article);

            var newEvent = new ModelCreatedEvent(article);

            eventRepository.Add(newEvent);

            OnArticleCreated(article);

            return article;
        }

        public event EventHandler<ArticleCreatedEventArgs>? ArticleCreated;

        private void OnArticleCreated(Article article)
        {
            ArticleCreated?.Invoke(this, new ArticleCreatedEventArgs(article));
        }

        public async Task DeleteArticleAsync(Article article, CancellationToken cancellationToken = default)
        {
            var hasItens = await articleRepository.HasItensAssociated(article.Guid, cancellationToken);

            if (hasItens)
            {
                throw new InvalidOperationException("Cannot delete article with associated items.");
            }

            articleRepository.Remove(article);

            var newEvent = new ModelDeletedEvent(article);

            eventRepository.Add(newEvent);

            OnArticleDeleted(article);
        }

        public event EventHandler<ArticleDeletedEventArgs>? ArticleDeleted;

        private void OnArticleDeleted(Article article)
        {
            ArticleDeleted?.Invoke(this, new ArticleDeletedEventArgs(article));
        }
    }
}
