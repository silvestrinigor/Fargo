using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents an article in the system.
    ///
    /// An article defines the descriptive information of a product or item type,
    /// such as its name and description. It does not represent a physical unit,
    /// but rather the metadata describing a type of item.
    /// </summary>
    public class Article : Entity
    {
        /// <summary>
        /// Gets or sets the name of the article.
        /// </summary>
        /// <remarks>
        /// The name uniquely identifies the article in the domain context
        /// and must satisfy the validation rules defined in <see cref="Name"/>.
        /// </remarks>
        public required Name Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the article.
        /// </summary>
        /// <remarks>
        /// If no description is provided, the default value is
        /// <see cref="Description.Empty"/>.
        /// </remarks>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;
    }
}