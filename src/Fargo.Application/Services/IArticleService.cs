using Fargo.Application.Solicitations.Commands.ArticleCommands;
using Fargo.Application.Solicitations.Queries.ArticleQueries;
using Fargo.Application.Solicitations.Responses;
using Fargo.Domain.Entities;

namespace Fargo.Application.Services
{
    public interface IArticleService
    {
        Task<Guid> CreateArticleAsync(ArticleCreateCommand command);
        Task<ArticleInformation?> GetArticleAsync(ArticleQuery getArticleQuery);
        Task DeleteArticleAsync(ArticleDeleteCommand command);
    }
}
