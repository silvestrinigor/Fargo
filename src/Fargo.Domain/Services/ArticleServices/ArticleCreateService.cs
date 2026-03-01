using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services.ArticleServices
{
    public class ArticleCreateService(
            IArticleRepository articleRepository
            )
    {
        public Article CreateArticle(
                User actor,
                Name name,
                bool isContainer
                )
        {
            actor.ValidatePermission(ActionType.CreateArticle);

            var article = new Article
            {
                Name = name,
                IsContainer = isContainer,
                UpdatedBy = actor
            };

            articleRepository.Add(article);

            return article;
        }
    }
}