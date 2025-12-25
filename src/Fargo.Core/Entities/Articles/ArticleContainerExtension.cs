using Fargo.Domain.Exceptions.Entities.Articles;
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
                ? throw new ArticleNegativeCapacityException()
                : value;
        }

        public Volume? VolumeCapacity
        {
            get;
            init => field =
                value is not null && value < Volume.Zero
                ? throw new ArticleNegativeCapacityException()
                : value;
        }

        public int? ItensQuantityCapacity
        { 
            get;
            init => field =
                value is not null && value < 0
                ? throw new ArticleNegativeCapacityException()
                : value;
        }
    }
}
