using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.ArticleModels
{
    public class ArticleReadModel
    {
        public required Guid Guid
        {
            get;
            init;
        }

        public required Name Name
        {
            get;
            init;
        }

        public required Description Description
        {
            get;
            init;
        }
    }
}