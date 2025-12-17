using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public class UnitArticle
    {
        public required Article Article { get; init; }

        public Mass? Mass { get; init; }

        public Length? Length { get; init; }

        public Length? Width { get; init; }

        public Length? Height { get; init; }

        public Volume? Volume
            => Length.HasValue && Width.HasValue && Height.HasValue
            ? Length.Value * Width.Value * Height.Value
            : null;

        public Density? Density
            => Mass.HasValue && Volume.HasValue
            ? Mass.Value / Volume.Value
            : null;

    }
}
