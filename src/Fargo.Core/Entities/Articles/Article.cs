using Fargo.Domain.Abstracts.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public partial class Article : Named
    {
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
        public Temperature? TemperatureMax { get; init; }
        public Temperature? TemperatureMin { get; init; }
    }
}
