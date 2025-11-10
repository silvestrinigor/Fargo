
namespace Fargo.Core.Entities.Abstracts;

/// <summary>
///
/// </summary>
public abstract class Entity : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; } = DateTime.Now;

    public Entity() : base()
    {
        Name = string.Empty;
    }

    public Entity(string name) : base()
    {
        Name = name;
    }

    public Entity(string name, Guid guid) : base(guid)
    {
        Name = name;
    }
}