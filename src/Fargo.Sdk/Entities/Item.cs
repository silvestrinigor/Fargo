namespace Fargo.Sdk.Entities;

public class Item : Entity
{
    public required Article Article
    {
        get;
        init;
    }
}
