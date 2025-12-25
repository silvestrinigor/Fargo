namespace Fargo.Domain.Exceptions.Entities.Articles
{
    public class ArticleNegativeCapacityException()
        : FargoException("Capacity value cannot be negative.");
}
