using UnitsNet;

namespace Fargo.Core.Articles;

/// <summary>
/// Defines container constraints for an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// A container article represents an article that may contain other articles,
/// optionally constrained by allowed articles, restricted articles, and maximum mass.
/// </remarks>
public sealed class ArticleContainer
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    /// <summary>
    /// Gets or sets the maximum mass allowed inside the container.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, no maximum mass constraint is defined.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than or equal to zero.
    /// </exception>
    public Mass? MaxMass { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleContainer() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ArticleContainer(Article article)
    {
        Article = article;
        ArticleGuid = article.Guid;
    }

    public void SetMaxMass(Mass? maxMass)
    {
        if (maxMass is not null && maxMass.Value <= Mass.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxMass),
                maxMass,
                "The maximum mass of a container must be greater than zero.");
        }

        MaxMass = maxMass;
    }
}
