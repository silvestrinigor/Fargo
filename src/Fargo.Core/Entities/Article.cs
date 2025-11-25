using Fargo.Domain.Abstracts.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class Article : NamedEntity
    {
        public Mass? Mass { get; private set; }
        public Length? Length { get; private set; }
        public Length? Width { get; private set; }
        public Length? Height { get; private set; }
        public Density? Density => Mass is not null && Volume is not null 
            ? Mass / Volume
            : null;
        public Volume? Volume => Length is not null && Width is not null && Height is not null
            ? Length * Width * Height
            : null;
        public TimeSpan? ShelfLife { get; set; }
    }
}