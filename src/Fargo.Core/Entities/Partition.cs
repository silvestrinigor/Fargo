using Fargo.Core.Collections;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities
{
    public class Partition : Entity
    {
        public IReadOnlyCollection<Entity> Entities => entities;

        internal readonly EntityCollection<Entity> entities = [];
    }
}


