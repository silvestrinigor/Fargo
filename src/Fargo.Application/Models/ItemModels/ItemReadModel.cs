namespace Fargo.Application.Models.ItemModels
{
    /// <summary>
    /// Represents the read model of an item used in query operations.
    /// </summary>
    /// <param name="Guid">
    /// The unique identifier of the item.
    /// </param>
    /// <param name="ArticleGuid">
    /// The unique identifier of the article associated with the item.
    /// </param>
    public record ItemReadModel(
            Guid Guid,
            Guid ArticleGuid
            );
}