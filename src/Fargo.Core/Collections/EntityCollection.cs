using Fargo.Core.Entities.Abstracts;
using System.Collections;

namespace Fargo.Core.Collections;

public class EntityCollection<TEntity> : IReadOnlyCollection<TEntity>, ICollection<TEntity> where TEntity : Entity
{
    private readonly ICollection<TEntity> entities = [];

    public int Count => entities.Count;

    public bool IsReadOnly => entities.IsReadOnly;

    public void Add(TEntity item)
    {
        entities.Add(item);
    }

    public void Clear()
    {
        entities.Clear();
    }

    public bool Contains(TEntity item)
    {
        return entities.Contains(item);
    }

    public void CopyTo(TEntity[] array, int arrayIndex)
    {
        entities.CopyTo(array, arrayIndex);
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        return entities.GetEnumerator();
    }

    public bool Remove(TEntity item)
    {
        return entities.Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return entities.GetEnumerator();
    }
}
