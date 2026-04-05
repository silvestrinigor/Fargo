using Fargo.Sdk.Entities;

namespace Fargo.Sdk.Articles;

public class Article : Entity
{
    internal Article() { }

    public string Name { get; internal init; } = string.Empty;

    public string Description { get; internal init; } = string.Empty;
}
