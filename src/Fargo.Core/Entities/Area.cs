using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities;

public class Area : Node
{
    public Area() : base() { }
    public Area(string name) : base(name) { }
    public Area(string name, Guid guid) : base(name, guid) { }
} 