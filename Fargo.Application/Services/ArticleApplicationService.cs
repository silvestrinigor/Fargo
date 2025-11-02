using Fargo.Application.Contracts;
using Fargo.Application.Contracts.UnitOfWork;
using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Core.Contracts;

namespace Fargo.Application.Services;

public class ArticleApplicationService(IArticleFactory articleFactory, IArticleRepository articleRepository, IUnitOfWork unitOfWork) : IArticleApplicationService
{
    private readonly IArticleRepository articleRepository = articleRepository;
    private readonly IArticleFactory articleFactory = articleFactory;
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    public async Task<ArticleDto?> GetArticleAsync(Guid guid)
    {
        var article = await articleRepository.GetAsync(guid);

        return article?.ToDto();
    }

    public async Task<ArticleDto> CreateArticleAsync(ArticleDto articleCreateDto) 
    {
        ArgumentNullException.ThrowIfNullOrEmpty(articleCreateDto.Name);

        var article = articleFactory.Create(articleCreateDto.Name);

        articleRepository.Add(article);

        await unitOfWork.SaveChangesAsync();

        return article.ToDto();
    }

    public async Task DeleteArticleAsync(Guid guid)
    {
        var article = await articleRepository.GetAsync(guid);

        ArgumentNullException.ThrowIfNull(article);

        articleRepository.Remove(article);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ArticleDto>> GetArticlesAsync()
    {
        var articles = await articleRepository.GetAsync();

        return articles.Select(article => article.ToDto());
    }

    public Task UpdateArticleAsync(ArticleDto articleUpdateDto)
    {
        throw new NotImplementedException();
    }
}
