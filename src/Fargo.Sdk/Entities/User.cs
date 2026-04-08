namespace Fargo.Sdk.Entities;

public class User : Entity
{
    internal User()
    {

    }

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

    public void SetPassword()
    {

    }
}
