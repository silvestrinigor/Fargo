using Fargo.Domain.Exceptions.Entities.Articles;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public class Article : Entity
    {
        internal Article(Name? name = null, Description? description = null) : base(name, description) { }

        public Article(IArticleService service, Name? name = null, Description? description = null) : this(name, description)
        {
            service.CreateNewEntity(this);
        }

        public TimeSpan? ShelfLife
        { 
            get;
            set
            {
                field 
                    = value is not null && value < TimeSpan.Zero
                    ? throw new ArticleNegativePropertyException()
                    : value;
            }
        }

        public Length? Length
        {
            get => field is not null
                ? field
                : IsLengthCalculableFromWidthHeightVolume
                ? Volume / Width / Height
                : null;

            set
            {
                field
                = field is null && IsLengthCalculableFromWidthHeightVolume
                ? throw new ArticleCalculatedRedundantValueSetException()
                : value;
            }
        }

        private bool IsLengthCalculableFromWidthHeightVolume
            => Width is not null && Height is not null && Volume is not null;

        public Length? Width
        {
            get => field is not null
                ? field
                : IsWidthCalculableFromLengthHeightVolume
                ? Volume / Length / Height
                : null;

            set => field
                = field is null && IsWidthCalculableFromLengthHeightVolume
                ? throw new ArticleCalculatedRedundantValueSetException()
                : value;
        }

        private bool IsWidthCalculableFromLengthHeightVolume
            => Length is not null && Height is not null && Volume is not null;

        public Length? Height 
        {
            get => field is not null
                ? field
                : IsHeightCalculableFromLengthWidthVolume
                ? Volume / Length / Width
                : null;

            set => field
                = field is null && IsHeightCalculableFromLengthWidthVolume
                ? throw new ArticleCalculatedRedundantValueSetException()
                : value;
        }

        private bool IsHeightCalculableFromLengthWidthVolume
            => Length is not null && Width is not null && Volume is not null;

        public Mass? Mass 
        {
            get => field is not null
                ? field
                : IsMassCalculableFromVolumeDensity
                ? Density * Volume
                : null;

            set => field
                = field is null && IsMassCalculableFromVolumeDensity
                ? throw new ArticleCalculatedRedundantValueSetException()
                : value;
        }

        private bool IsMassCalculableFromVolumeDensity
            => Volume is not null && Density is not null;

        public Volume? Volume
        {
            get => field is not null
                ? field
                : IsVolumeCalculableFromLengthWidthHeight
                ? Length * Width * Height
                : IsVolumeCalculableFromMassDensity
                ? Mass / Density
                : null;

            set => field
                = field is null && IsVolumeCalculableFromLengthWidthHeight || IsVolumeCalculableFromMassDensity
                ? throw new ArticleCalculatedRedundantValueSetException()
                : value;
        }

        private bool IsVolumeCalculableFromLengthWidthHeight
            => Length is not null && Width is not null && Height is not null;

        private bool IsVolumeCalculableFromMassDensity
            => Mass is not null && Density is not null;

        public Density? Density
        {
            get => field is not null 
                ? field 
                : IsDensityCalculableFromMassVolume
                ? Mass / Volume
                : null;

            set => field
                = field is null && IsDensityCalculableFromMassVolume
                ? throw new ArticleCalculatedRedundantValueSetException()
                : value;
        }

        private bool IsDensityCalculableFromMassVolume
            => Volume is not null && Mass is not null;

        public ArticleContainerInformation? ContainerInformation { get; init; }

        public bool IsContainer => ContainerInformation is not null;
    }
}
