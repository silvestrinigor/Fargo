using Fargo.Domain.Abstracts.Entities;
using Fargo.Domain.Enuns;
using Fargo.Domain.Utils;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class Article : Entity
    {
        public double? MassValue { get; init; }
        public MassUnit MassUnit { get; init; } = MassUnit.Kilogram;
        public Mass? Mass => MassValue.HasValue ? MassUtils.Convert(MassValue.Value, MassUnit) : null;
        public double? LengthValue { get; init; }
        public LengthUnit LengthUnit { get; init; } = LengthUnit.Meter;
        public Length? Length => LengthValue.HasValue ? LengthUtils.Convert(LengthValue.Value, LengthUnit) : null;
        public double? WidthValue { get; init; }
        public LengthUnit WidthUnit { get; init; } = LengthUnit.Meter;
        public Length? Width => WidthValue.HasValue ? LengthUtils.Convert(WidthValue.Value, WidthUnit) : null;
        public double? HeightValue { get; init; }
        public LengthUnit HeightUnit { get; init; } = LengthUnit.Meter;
        public Length? Height => HeightValue.HasValue ? LengthUtils.Convert(HeightValue.Value, HeightUnit) : null;
        public bool SwitchStandingLyingPositions { get; init; } = false;
        public TimeSpan? ShelfLife { get; init; }
        public bool IsContainer { get; init; } = false;
        public bool IsMoveable { get; init; } = true;
        public double? ContainerMassCapacityValue { get; set; }
        public MassUnit ContainerMassCapacityUnit { get; set; } = MassUnit.Kilogram;
        public Mass? ContainerMassCapacity => ContainerMassCapacityValue.HasValue ? MassUtils.Convert(ContainerMassCapacityValue.Value, ContainerMassCapacityUnit) : null;
        public double? ContainerLengthCapacityValue { get; init; }
        public LengthUnit ContainerLengthCapacityUnit { get; init; } = LengthUnit.Meter;
        public Length? ContainerLengthCapacity => ContainerLengthCapacityValue.HasValue ? LengthUtils.Convert(ContainerLengthCapacityValue.Value, ContainerLengthCapacityUnit) : null;
        public double? ContainerWidthCapacityValue { get; init; }
        public LengthUnit ContainerWidthCapacityUnit { get; init; } = LengthUnit.Meter;
        public Length? ContainerWidthCapacity => ContainerWidthCapacityValue.HasValue ? LengthUtils.Convert(ContainerWidthCapacityValue.Value, ContainerWidthCapacityUnit) : null;
        public double? ContainerHeightCapacityValue { get; init; }
        public LengthUnit ContainerHeightCapacityUnit { get; init; } = LengthUnit.Meter;
        public Length? ContainerHeightCapacity => ContainerHeightCapacityValue.HasValue ? LengthUtils.Convert(ContainerHeightCapacityValue.Value, ContainerHeightCapacityUnit) : null;
    }
}