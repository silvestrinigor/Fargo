using Fargo.Core.Collections;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities;

/// <summary>  
/// Container is a entity that can hold other entities.
/// A entity can be in only one container at a time.
/// </summary>
public class Container : Entity
{
    public IEnumerable<Entity> Entities => entities;

    internal readonly EntityCollection<Entity> entities = [];

    public Container() : base() { }
    public Container(string name) : base(name) { }
    public Container(string name, Guid guid) : base(name, guid) { }
}
