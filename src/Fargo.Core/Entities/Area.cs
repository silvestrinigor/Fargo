using Fargo.Core.Entities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fargo.Core.Entities;

public class Area : DetailedEntity
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