namespace Fargo.Sdk.Entities;

public class UserGroup : Entity
{
    public required string Nameid
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
