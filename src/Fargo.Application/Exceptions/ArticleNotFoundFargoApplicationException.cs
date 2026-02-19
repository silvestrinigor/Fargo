namespace Fargo.Application.Exceptions
{
    public class ArticleNotFoundFargoApplicationException(Guid articleGuid)
        : FargoApplicationException()
    {
        public Guid ArticleGuid { get; } = articleGuid;
    }
}