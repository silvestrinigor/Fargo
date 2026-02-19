namespace Fargo.Domain.Exceptions
{
    public class ArticleNotFoundException(Guid articleGuid)
        : FargoException
    {
        public Guid ArticleGuid 
        {
            get;
        } = articleGuid;
    }
}