using Fargo.Sdk.Entities;

namespace Fargo.Sdk.Items;

public class Item : Entity
{
    internal Item() { }

    public Guid ArticleGuid { get; internal init; }
}
