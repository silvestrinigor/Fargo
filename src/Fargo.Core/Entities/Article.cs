using Fargo.Domain.Abstracts.Entities;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Domain.Entities
{
    public class Article : Entity
    {
        public decimal? Mass { get; init; }
        public MassUnit MassUnit { get; init; } = MassUnit.Kilogram;
        public decimal? Length { get; init; }
        public LengthUnit LengthUnit { get; init; } = LengthUnit.Meter;
        public decimal? Width { get; init; }
        public LengthUnit WidthUnit { get; init; } = LengthUnit.Meter;
        public decimal? Height { get; init; }
        public LengthUnit HeightUnit { get; init; } = LengthUnit.Meter;
        public TimeSpan? ShelfLife { get; init; }
    }
}