namespace Fargo.Core.Entities.Abstracts;

/// <summary>
///
/// </summary>
public abstract class DetailedEntity : Entity
{
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; } = DateTime.Now;

    public DetailedEntity() : base()
    {
        Name = string.Empty;
    }

    public DetailedEntity(string name) : base()
    {
        Name = name;
    }

    public DetailedEntity(string name, Guid guid) : base(guid)
    {
        Name = name;
    }
}