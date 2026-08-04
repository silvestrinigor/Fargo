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
    /// <summary>
    /// Gets the unique identifier of the associated article.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article associated with these container constraints.
    /// </summary>
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

    /// <summary>
    /// Initializes a new <see cref="ArticleContainer"/> instance.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleContainer() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes the container constraints for the specified article.
    /// </summary>
    /// <param name="article">
    /// The container article.
    /// </param>
    internal ArticleContainer(Article article)
    {
        Article = article;
        ArticleGuid = article.Guid;
    }

    /// <summary>
    /// Sets the maximum mass allowed inside the container.
    /// </summary>
    /// <param name="maxMass">
    /// The maximum allowed mass, or <see langword="null"/> to remove the constraint.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxMass"/> is less than or equal to zero.
    /// </exception>
    public void SetMaxMass(Mass? maxMass)
    {
        if (maxMass is not null && maxMass.Value <= Mass.Zero)
        {
            throw new FargoCoreException(
                "The maximum mass of a container must be greater than zero.",
                FargoCoreErrorType.InvalidArgument);
        }

        MaxMass = maxMass;
    }
}
