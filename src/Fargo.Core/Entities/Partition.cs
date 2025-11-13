using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities;

public class Partition : Node
{
    public Partition() : base() { }
    public Partition(string name) : base(name) { }
    public Partition(string name, Guid guid) : base(name, guid) { }
}
