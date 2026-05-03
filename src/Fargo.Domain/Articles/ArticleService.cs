namespace Fargo.Domain.Articles;

public class ArticleService(IArticleRepository articleRepository)
{
    public async Task DeleteArticle(Article article, CancellationToken cancellationToken = default)
    {
        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken
        );

        if (hasItems)
        {
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);
    }
}
