namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents an item in the system.
    ///
    /// An item is an individual instance associated with a specific
    /// <see cref="Article"/>. While an <see cref="Article"/> defines the
    /// descriptive information of a product, an <see cref="Item"/> represents
    /// a concrete unit of that article.
    /// </summary>
    public class Item : Entity
    {
        /// <summary>
        /// Gets the unique identifier of the associated <see cref="Article"/>.
        /// </summary>
        public Guid ArticleGuid
        {
            get;
            private init;
        }

        /// <summary>
        /// Gets the article associated with this item.
        ///
        /// When the article is assigned, the <see cref="ArticleGuid"/>
        /// is automatically synchronized with the article's identifier.
        /// </summary>
        public required Article Article
        {
            get;
            init
            {
                ArticleGuid = value.Guid;
                field = value;
            }
        }
    }
}