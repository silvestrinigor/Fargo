using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public class Article : OptionalNamedEntity
    {
        public Length? Length
        {
            get => field is not null
                ? field
                : Width is not null && Height is not null && Volume is not null
                ? Volume / Width / Height
                : null;

            init => field 
                = Width is not null && Height is not null && Volume is not null
                ? throw new InvalidOperationException()
                : value;
        }

        public Length? Width
        {
            get => field is not null
                ? field
                : Length is not null && Height is not null && Volume is not null
                ? Volume / Length / Height
                : null;

            init => field 
                = Length is not null && Height is not null && Volume is not null
                ? throw new InvalidOperationException()
                : value;
        }

        public Length? Height 
        {
            get => field is not null
                ? field
                : Length is not null && Width is not null && Volume is not null
                ? Volume / Length / Width
                : null;

            init => field 
                = Length is not null && Width is not null && Volume is not null
                ? throw new InvalidOperationException()
                : value;
        }

        public Mass? Mass 
        {
            get => field is not null
                ? field
                : Volume is not null && Density is not null
                ? Density * Volume
                : null;

            init => field 
                = Volume is not null && Density is not null
                ? throw new InvalidOperationException()
                : value;
        }

        public Volume? Volume
        {
            get => field is not null
                ? field
                : Length is not null && Width is not null && Height is not null
                ? Length * Width * Height
                : Mass is not null && Density is not null
                ? Mass / Density
                : null;

            init => field 
                = Length is not null && Width is not null && Height is not null
                ? throw new InvalidOperationException()
                : Mass is not null && Density is not null
                ? throw new InvalidOperationException()
                : value;
        }

        public Density? Density
        {
            get => field is not null 
                ? field 
                : Mass / Volume;

            init => field 
                = Volume is not null && Mass is not null
                ? throw new InvalidOperationException()
                : value;
        }
    
        public ArticleContainerExtension? Container
        {
            get;
            init;
        }

        public bool IsContainer => Container is not null;
    }
}
