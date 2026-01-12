namespace Fargo.Application.Exceptions
{
    public class ArticleNotFoundException(Guid articleGuid) 
        : FargoApplicationException($"Article {articleGuid} not found.");
}
