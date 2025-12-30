namespace Fargo.Domain.Exceptions.Entities.Articles
{
    public class ArticleNegativePropertyException()
        : FargoException("Property value cannot be negative.");
}
