using Fargo.Core.Collections;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities;

public class Area : Entity
{
    public IReadOnlyCollection<Entity> Entities => entities;
    internal readonly EntityCollection<Entity> entities = [];

    public Area() : base() { }
    public Area(string name) : base(name) { }
    public Area(string name, Guid guid) : base(name, guid) { }
}