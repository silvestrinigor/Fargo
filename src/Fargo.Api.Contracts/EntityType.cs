namespace Fargo.Api.Contracts;

/// <summary>Represents the type of entity involved in an event.</summary>
public enum EntityType
{
    Article = 0,
    Item = 1,
    User = 2,
    UserGroup = 3,
    Partition = 4,
    ApiClient = 5,
}
