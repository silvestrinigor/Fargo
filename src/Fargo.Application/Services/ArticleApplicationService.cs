using Fargo.Application.Contracts;
using Fargo.Application.Contracts.Persistence;
using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Core.Contracts;
using Fargo.Core.Entities;

namespace Fargo.Application.Services;

public class ArticleApplicationService(IArticleRepository articleRepository, IUnitOfWork unitOfWork) : IArticleApplicationService
{
    private readonly IArticleRepository articleRepository = articleRepository;
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    public async Task<EntityDto?> GetArticleAsync(Guid guid)
    {
        var article = await articleRepository.GetAsync(guid);

        return article?.ToEntityDto();
    }

    public async Task<EntityDto> CreateArticleAsync(EntityCreateDto articleCreateDto)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(articleCreateDto.Name);

        var article = new Article(articleCreateDto.Name);

        articleRepository.Add(article);

        await unitOfWork.SaveChangesAsync();

        return article.ToEntityDto();
    }

    public async Task DeleteArticleAsync(Guid guid)
    {
        var article = await articleRepository.GetAsync(guid);

        ArgumentNullException.ThrowIfNull(article);

        articleRepository.Remove(article);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<EntityDto>> GetArticlesAsync()
    {
        var articles = await articleRepository.GetAsync();

        return articles.Select(article => article.ToEntityDto());
    }

    public async Task UpdateArticleAsync(Guid articleGuid, EntityUpdateDto articleUpdateDto)
    {
        var article = await articleRepository.GetAsync(articleGuid);

        ArgumentNullException.ThrowIfNull(article);

        article.UpdateEntityProperties(articleUpdateDto);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<Guid>> GetArticlesGuidsAsync()
    {
        var articlesGuids = await articleRepository.GetGuidsAsync();

        return articlesGuids;
    }
}
