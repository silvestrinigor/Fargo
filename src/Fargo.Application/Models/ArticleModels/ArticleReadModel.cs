using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.ArticleModels
{
    /// <summary>
    /// Represents the read model of an article used in query operations.
    /// </summary>
    /// <remarks>
    /// This model is part of the query side (CQRS) and is used to transfer
    /// article data from the persistence layer to the application or API.
    /// </remarks>
    public class ArticleReadModel
    {
        /// <summary>
        /// Gets the unique identifier of the article.
        /// </summary>
        public required Guid Guid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the name of the article.
        /// </summary>
        public required Name Name
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the description of the article.
        /// </summary>
        public required Description Description
        {
            get;
            init;
        }
    }
}