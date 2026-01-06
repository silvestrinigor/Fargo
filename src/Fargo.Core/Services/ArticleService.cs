using Fargo.Domain.Entities;
using Fargo.Domain.Events;
using Fargo.Domain.Extensions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.EventsValueObjects;

namespace Fargo.Domain.Services
{
    public class ArticleService
    {
        private readonly IArticleRepository articleRepository;

        private readonly IEventRepository eventRepository;

        public ArticleService(IArticleRepository articleRepository, IEventRepository eventRepository)
        {
            this.articleRepository = articleRepository;

            this.eventRepository = eventRepository;

            ArticleCreated += HandleArticleCreated;

            ArticleDeleted += HandleArticleDeleted;
        }

        public async Task<Article?> GetArticle(Guid articleGuid, CancellationToken cancellationToken = default)
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

            OnArticleCreated(article);

            return article;
        }

        public event EventHandler<ArticleCreatedEventArgs>? ArticleCreated;

        private void OnArticleCreated(Article article)
        {
            ArticleCreated?.Invoke(this, new ArticleCreatedEventArgs(article));
        }

        private void HandleArticleCreated(object? sender, ArticleCreatedEventArgs e)
        {
            var newEvent = new Event(e);

            eventRepository.Add(newEvent);
        }

        public async Task DeleteArticleAsync(Article article, CancellationToken cancellationToken = default)
        {
            var hasItens = await articleRepository.HasItensAssociated(article.Guid, cancellationToken);

            if (hasItens)
            {
                throw new InvalidOperationException("Cannot delete article with associated items.");
            }

            articleRepository.Remove(article);

            OnArticleDeleted(article);
        }

        public event EventHandler<ArticleDeletedEventArgs>? ArticleDeleted;

        private void OnArticleDeleted(Article article)
        {
            ArticleDeleted?.Invoke(this, new ArticleDeletedEventArgs(article));
        }

        private void HandleArticleDeleted(object? sender, ArticleDeletedEventArgs e)
        {
            var newEvent = new Event(e);

            eventRepository.Add(newEvent);
        }
    }
}
