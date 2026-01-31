using Fargo.Domain.Entities;

namespace Fargo.Domain.Exceptions
{
    public class ArticleDeleteWithItemsAssociatedException(Article article)
        : FargoException
    {
        public Article Article 
        {
            get;
        } = article;
    }
}