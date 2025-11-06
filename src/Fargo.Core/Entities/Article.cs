namespace Fargo.Core.Entities;

/// <summary>  
/// Represents a particular item.
/// </summary>
public class Article : Entity
{
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; } = DateTime.Now;

    public Article() : base()
    {
        Name = string.Empty;
    }

    public Article(string name) : base()
    {
        Name = name;
    }

    public Article(string name, Guid guid) : base(guid)
    {
        Name = name;
    }
}