using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities
{
    public class Area : Entity 
    {
        public bool IsGlobalArea { get; init; } = false;
    }
}