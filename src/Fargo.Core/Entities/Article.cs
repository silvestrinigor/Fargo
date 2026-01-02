using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a physical article or item with associated descriptive, dimensional, and physical properties.
    /// </summary>
    /// <remarks>
    /// The Article class provides properties for common physical characteristics such as dimensions,
    /// mass, volume, density, and shelf life, as well as container information. Some properties may be automatically
    /// calculated from others if not explicitly set. For example, if length, width, and height are provided, the volume
    /// can be derived automatically. Attempting to set a property that is currently calculable from other properties
    /// will result in an exception. This class is intended for use in scenarios where detailed tracking of an item's
    /// physical attributes is required, such as inventory or logistics systems.
    /// </remarks>
    public class Article : Entity
    {
        public Article() : base(EntityType.Article)
        {
        }

        public Article(Name? name, Description? description = null) : base(EntityType.Article)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Gets or sets the maximum duration for which the item remains usable or safe to use.
        /// </summary>
        public TimeSpan? ShelfLife
        {
            get;
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(ShelfLife), "Cannot be negative.");
                }

                field = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowable temperature for the container.
        /// </summary>
        /// <remarks>The value must not exceed the value of <see cref="MaximumContainerTemperature"/>.
        /// Setting this property to a value greater than <see cref="MaximumContainerTemperature"/> will throw an
        /// exception.</remarks>
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

        /// <summary>
        /// Gets or sets the maximum allowable temperature for the container.
        /// </summary>
        /// <remarks>Setting this property to a value lower than <see cref="MinimumContainerTemperature"/>
        /// will result in an <see cref="ArgumentOutOfRangeException"/>. Use this property to enforce upper temperature
        /// limits for container contents.</remarks>
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

        /// <summary>
        /// Gets or sets the length of the object along the X-axis.
        /// </summary>
        /// <remarks>
        /// If the length can be calculated from the width, height, and volume, this property
        /// returns the computed value and cannot be set directly. Otherwise, the value is stored and retrieved as
        /// specified.
        /// </remarks>
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
                if (field is null && IsLengthCalculableFromWidthHeightVolume)
                {
                    throw new InvalidOperationException("Cannot set length x when the value is calculable from other properties.");
                }

                field = value;
            }
        }

        private bool IsLengthCalculableFromWidthHeightVolume
            => LengthY is not null && LengthZ is not null && Volume is not null;

        /// <summary>
        /// Gets or sets the length of the object along the Y-axis.
        /// </summary>
        /// <remarks>
        /// If the length along the Y-axis can be calculated from the volume and the other
        /// dimensions, this property returns the computed value. Attempting to set this property when it is calculable
        /// from other properties will result in an exception.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the length of the object along the Z-axis, if specified or calculable.
        /// </summary>
        /// <remarks>
        /// If the value is not explicitly set and the height can be calculated from the volume
        /// and the X and Y lengths, this property returns the computed value. Attempting to set this property when the
        /// value is calculable from other properties will result in an exception.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the volume of the object, if it is not calculable from other properties.
        /// </summary>
        /// <remarks>
        /// If the volume can be determined from the object's dimensions (length, width, and
        /// height) or from its mass and density, this property returns the calculated value and cannot be set directly.
        /// Attempting to set the volume when it is calculable from other properties will result in an
        /// exception.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the mass of the object, if specified or calculable from other properties.
        /// </summary>
        /// <remarks>
        /// If the mass is not explicitly set and both density and volume are available, the mass
        /// is calculated as the product of density and volume. Attempting to set the mass when it is calculable from
        /// density and volume will result in an exception.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the density value for the object, if explicitly specified.
        /// </summary>
        /// <remarks>If the density is not explicitly set and can be calculated from mass and volume, this
        /// property returns the computed value. Attempting to set the density when it is calculable from other
        /// properties will result in an exception.</remarks>
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

        /// <summary>
        /// Gets or sets on init the container-specific information for this article, if it functions as a container.
        /// </summary>
        public ArticleContainer? Container { get; init; }

        /// <summary>
        /// Gets a value indicating whether this item represents a container.
        /// </summary>
        public bool IsContainer => Container is not null;
    }


    public class ArticleContainer
    {
        /// <summary>
        /// Gets or sets the maximum mass capacity allowed.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the maximum volume capacity.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the maximum allowed quantity of items.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the default temperature of the container.
        /// </summary>
        public Temperature? DefaultTemperature
        {
            get;
            set;
        }
    }
}
