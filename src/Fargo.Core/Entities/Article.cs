using Fargo.Domain.Abstracts.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class Article : Entity
    {
        public Mass? Mass { get; init; }
        public Length? Length { get; init; }
        public Length? Width { get; init; }
        public Length? Height { get; init; }
        public Volume? Volume => Length is not null && Width is not null && Height is not null
            ? Length * Width * Height
            : null;
        public Density? Density => Mass is not null && Volume is not null
            ? Mass / Volume
            : null;
        public TimeSpan? ShelfLife { get; init; }
    }

    // types of set:
    // Bulk - When the article can't be understand as a unit
    // Item - When the article can be understand as a unit
    // Bunch - A bunch of itens of the same article
    //   - If     
}