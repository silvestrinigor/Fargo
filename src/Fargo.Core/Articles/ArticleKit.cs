namespace Fargo.Core.Articles;

/// <summary>
/// Defines an article kit relationship.
/// </summary>
/// <remarks>
/// A kit article is composed of one or more components.
/// Each component defines the source article and the quantity included in the kit.
/// </remarks>
public sealed class ArticleKit
{
    private ArticleKit()
    {
    }

    public ArticleKit(IReadOnlyCollection<ArticleKitComponent> components)
    {
        SetComponents(components);
    }

    /// <summary>
    /// Gets the components that compose the kit.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains null components, or contains duplicated source articles.
    /// </exception>
    public IReadOnlyCollection<ArticleKitComponent> Components { get; private set; } = [];

    private void SetComponents(IReadOnlyCollection<ArticleKitComponent> components)
    {
        if (components.Any(component => component is null))
        {
            throw new ArgumentException(
                "A kit cannot contain null components.",
                nameof(components));
        }

        if (components
            .GroupBy(component => component.ArticleGuid)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "A kit cannot contain duplicated source articles.",
                nameof(components));
        }

        Components = components;
    }
}
