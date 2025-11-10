using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities;

public class Area : Entity
{
    private readonly List<Guid> ChildAreas = [];

    public Area() : base()
    {
    }

    public Area(string name) : base(name)
    {
    }

    public Area(string name, Guid guid) : base(name, guid)
    {
    }
}