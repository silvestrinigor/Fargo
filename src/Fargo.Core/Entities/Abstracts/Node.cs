using Fargo.Core.Collections;

namespace Fargo.Core.Entities.Abstracts
{
    public abstract class Node : Entity
    {
        public IReadOnlyCollection<Entity> Entities => entities;

        internal readonly EntityCollection<Entity> entities = [];

        public Node() : base() { }
        public Node(string name) : base(name) { }
        public Node(string name, Guid guid) : base(name, guid) { }
    }
}
