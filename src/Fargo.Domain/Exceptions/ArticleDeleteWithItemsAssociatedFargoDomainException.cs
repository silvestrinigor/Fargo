using Fargo.Domain.Entities;

namespace Fargo.Domain.Exceptions
{
    public class ArticleDeleteWithItemsAssociatedFargoDomainException(Article article)
        : FargoDomainException
    {
        public Article Article
        {
            get;
        } = article;
    }
}