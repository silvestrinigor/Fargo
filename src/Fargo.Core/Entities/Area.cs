using Fargo.Core.Entities.Abstracts;
using Fargo.Core.Enums;

namespace Fargo.Core.Entities
{
    public class Area : Entity 
    {
        public bool IsGlobalArea { get; init; } = false;

        public Area()
        {
            EntityType = EEntityType.Area;
        }
    }
}