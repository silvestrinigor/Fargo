using Fargo.Core.Contracts;
using Fargo.Core.Entities;

namespace Fargo.Core.Factories;

public class ArticleFactory : IArticleFactory
{
    public Article Create(string name)
    {
        return new(name);
    }

    public void Create(string name, out Article article)
    {
        article = Create(name);
    }
}
