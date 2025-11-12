using Fargo.Core.Collections;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities;

public class Partition : Entity
{
    public IReadOnlyCollection<Entity> Entities => entities;
    internal readonly EntityCollection<Entity> entities = [];

    public Partition() : base() { }
    public Partition(string name) : base(name) { }
    public Partition(string name, Guid guid) : base(name, guid) { }
}
