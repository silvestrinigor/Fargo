using Fargo.Core.Entities;

namespace Fargo.Core.Contracts;

public interface IArticleFactory
{
    Article Create(string name);
    void Create(string name, out Article article);
}
