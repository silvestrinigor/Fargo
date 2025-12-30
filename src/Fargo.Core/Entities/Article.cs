using Fargo.Domain.ValueObjects;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class Article : Entity
    {
        public Article(Name? name = null, Description? description = null) : base(name, description) { }

        public TimeSpan? ShelfLife
        {
            get;
            set
            {
                if (value is not null && value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(ShelfLife), "Cannot be negative.");
                }

                field = value;
            }
        }

        public Length? LengthX
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                if (IsLengthCalculableFromWidthHeightVolume)
                {
                    return Volume / LengthY / LengthZ;
                }
                
                return null;
            }

            set
            {
                if (field is not null && IsLengthCalculableFromWidthHeightVolume)
                {
                    throw new InvalidOperationException("Cannot set length x when the value is calculable from other properties.");
                }

                field = value;
            }
        }

        private bool IsLengthCalculableFromWidthHeightVolume
            => LengthY is not null && LengthZ is not null && Volume is not null;

        public Length? LengthY
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                if (IsWidthCalculableFromLengthHeightVolume)
                {
                    return Volume / LengthX / LengthZ;
                }

                return null;
            }

            set
            {
                if (field is null && IsWidthCalculableFromLengthHeightVolume)
                {
                    throw new InvalidOperationException("Cannot set length y when the value is calculable from other properties.");
                }

                field = value;
            }
        }

        private bool IsWidthCalculableFromLengthHeightVolume
            => LengthX is not null && LengthZ is not null && Volume is not null;

        public Length? LengthZ 
        {
            get
            {
                if (field is not null)
                {
                    return field; 
                }

                if (IsHeightCalculableFromLengthWidthVolume)
                {
                    return Volume / LengthX / LengthY;
                }

                return null;
            }

            set
            {
                if (field is null && IsHeightCalculableFromLengthWidthVolume)
                {
                    throw new InvalidOperationException("Cannot set length z when the value is calculable from other properties.");
                }

                field = value;
            }
        }

        private bool IsHeightCalculableFromLengthWidthVolume
            => LengthX is not null && LengthY is not null && Volume is not null;

        public Volume? Volume
        {
            get
            {
                if (field is not null)
                {
                    return field; 
                }

                if (IsVolumeCalculableFromLengthWidthHeight)
                {
                    return LengthX * LengthY * LengthZ;
                }

                if (IsVolumeCalculableFromMassDensity)
                {
                    return Mass / Density;
                }

                return null;
            }

            set
            {
                if (field is null && IsVolumeCalculableFromLengthWidthHeight || IsVolumeCalculableFromMassDensity)
                {
                    throw new InvalidOperationException("Cannot set volume when the value is calculable from other properties.");
                }

                field = value;
            }
        }

        private bool IsVolumeCalculableFromLengthWidthHeight
            => LengthX is not null && LengthY is not null && LengthZ is not null;

        private bool IsVolumeCalculableFromMassDensity
            => Mass is not null && Density is not null;

        public Mass? Mass 
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                if (IsMassCalculableFromVolumeDensity)
                {
                    return Density * Volume;
                }

                return null;
            }

            set
            {
                if (field is null && IsMassCalculableFromVolumeDensity)
                {
                    throw new InvalidOperationException("Cannot set mass when the value is calculable from other properties.");
                }

                field = value;
            }
        }

        private bool IsMassCalculableFromVolumeDensity
            => Volume is not null && Density is not null;

        public Density? Density
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                if (IsDensityCalculableFromMassVolume)
                {
                    return Mass / Volume;
                }

                return null;
            }

            set
            {
                if (field is null && IsDensityCalculableFromMassVolume)
                {
                    throw new InvalidOperationException("Cannot set density when the value is calculable from other properties.");
                }

                field = value;
            }
        }

        private bool IsDensityCalculableFromMassVolume
            => Volume is not null && Mass is not null;

        public Temperature? MinimumContainerTemperature 
        { 
            get;
            set
            {
                if (value > MaximumContainerTemperature)
                {
                    throw new ArgumentOutOfRangeException(nameof(MinimumContainerTemperature), "Cannot be bigger than maximum container temperature.");
                }

                field = value;
            }
        }

        public Temperature? MaximumContainerTemperature
        { 
            get; 
            set
            {
                if (value < MinimumContainerTemperature)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaximumContainerTemperature), "Cannot be lower than minimum container temperature.");
                }

                field = value;
            }
        }

        public ArticleContainerInformation? ContainerInformation { get; init; }

        public bool IsContainer => ContainerInformation is not null;
    }

    public class ArticleContainerInformation
    {
        public Mass? MassCapacity
        {
            get;
            set
            {
                if (value < Mass.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(MassCapacity), "Cannot be negative.");
                }

                field = value;
            }
        }

        public Volume? VolumeCapacity
        {
            get;
            set
            {
                if (value < Volume.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(VolumeCapacity), "Cannot be negative.");
                }

                field = value;
            }
        }

        public int? ItensQuantityCapacity
        {
            get;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ItensQuantityCapacity), "Cannot be negative.");
                }

                field = value;
            }
        }

        public Temperature? DefaultTemperature
        {
            get;
            set;
        }
    }
}
