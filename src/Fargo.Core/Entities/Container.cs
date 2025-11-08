using Fargo.Core.Entities.Abstracts;
using System.Collections.ObjectModel;

namespace Fargo.Core.Entities;

/// <summary>  
/// Container is a entity that can hold other entities.
/// A entity can be in only one container at a time.
/// </summary>
public class Container : DetailedEntity
{
    public ReadOnlyCollection<Guid> ChildEntities => childEntities.ToList().AsReadOnly();

    private readonly HashSet<Guid> childEntities = [];

    public Container() : base()
    {
    }

    public Container(string name) : base(name)
    {
    }

    public Container(string name, Guid guid) : base(name, guid)
    {
    }

    internal void AddChildEntity(Guid entityGuid)
    {
        childEntities.Add(entityGuid);
    }

    internal void RemoveChildEntity(Guid entityGuid)
    {
        childEntities.Remove(entityGuid);
    }
}
