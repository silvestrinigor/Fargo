using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities;

/// <summary>  
/// Represents a particular item.
/// </summary>
public class Article : Entity
{
    public Article() : base() { }
    public Article(string name) : base(name) { }
    public Article(string name, Guid guid) : base(name, guid) { }
}