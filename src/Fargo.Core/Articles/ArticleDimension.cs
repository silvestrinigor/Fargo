using UnitsNet;

namespace Fargo.Core.Articles;

public class ArticleDimension
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    public Length? X { get; private set; }

    public Length? Y { get; private set; }

    public Length? Z { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleDimension() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ArticleDimension(Article article)
    {
        Article = article;
        ArticleGuid = article.Guid;
    }

    public void SetDimensions(Length? lengthX, Length? lengthY, Length? lengthZ)
    {
        X = lengthX;
        Y = lengthY;
        Z = lengthZ;
    }
}
