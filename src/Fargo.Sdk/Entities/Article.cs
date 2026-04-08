namespace Fargo.Sdk.Entities;

public class Article : Entity
{
    internal Article()
    {

    }

    public required string Name
    {
        get;
        set;
    }

    public required string Description
    {
        get;
        set;
    }
}
