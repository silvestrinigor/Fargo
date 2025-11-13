using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities;

public class Container : Node
{
    public Container() : base() { }
    public Container(string name) : base(name) { }
    public Container(string name, Guid guid) : base(name, guid) { }
}
