using UnitsNet;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// The physical dimensions of an <see cref="Article"/>; each axis is optional.
/// </summary>
public class ArticleDimension
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    public Length? X { get; private set; }

    public Length? Y { get; private set; }

    public Length? Z { get; private set; }

    // Required by EF Core.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleDimension() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ArticleDimension(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        Article = article;
        ArticleGuid = article.Guid;
    }

    /// <summary>
    /// Replaces all three axes; passing <see langword="null"/> removes that measurement.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A given dimension is less than or equal to zero.
    /// </exception>
    internal void SetDimensions(Length? lengthX, Length? lengthY, Length? lengthZ)
    {
        ValidateDimension(lengthX, nameof(lengthX));
        ValidateDimension(lengthY, nameof(lengthY));
        ValidateDimension(lengthZ, nameof(lengthZ));

        X = lengthX;
        Y = lengthY;
        Z = lengthZ;
    }

    private static void ValidateDimension(Length? length, string paramName)
    {
        if (length is not null && length.Value <= Length.Zero)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                length,
                "Dimensions must be greater than zero.");
        }
    }
}
