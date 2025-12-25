using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public class ArticleContainerExtension
    {
        public Mass? MassCapacity
        {
            get;
            init => field =
                value is not null && value < Mass.Zero
                ? throw new InvalidOperationException()
                : value;
        }

        public Volume? VolumeCapacity
        {
            get;
            init => field =
                value is not null && value < Volume.Zero
                ? throw new InvalidOperationException()
                : value;
        }
    }
}
