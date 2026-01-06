using Fargo.Domain.Entities;
using Fargo.Domain.Events;
using Fargo.Domain.Extensions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.EventsValueObjects;
using System.Text.Json;

namespace Fargo.Domain.Services
{
    public class ArticleService
    {
        private readonly IArticleRepository articleRepository;

        private readonly IEventRepository eventRepository;

        private readonly JsonSerializerOptions? jsonSerializerOptions;

        public ArticleService(IArticleRepository articleRepository, IEventRepository eventRepository, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            this.articleRepository = articleRepository;

            this.eventRepository = eventRepository;

            this.jsonSerializerOptions = jsonSerializerOptions;

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
            var newEvent = new Event(e, jsonSerializerOptions);

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
