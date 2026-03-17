namespace Fargo.Application.Models.ItemModels;

/// <summary>
/// Represents the data required to create a new item.
/// </summary>
/// <param name="ArticleGuid">
/// The unique identifier of the article associated with the item.
/// </param>
public record ItemCreateModel(
        Guid ArticleGuid
        );
