using Fargo.Domain.Exceptions.Entities.Articles;
using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public class ArticleContainerInformation
    {
        public Mass? MassCapacity
        {
            get;
            set => field =
                value is not null && value < Mass.Zero
                ? throw new ArticleNegativePropertyException()
                : value;
        }

        public Volume? VolumeCapacity
        {
            get;
            set => field =
                value is not null && value < Volume.Zero
                ? throw new ArticleNegativePropertyException()
                : value;
        }

        public int? ItensQuantityCapacity
        { 
            get;
            set => field =
                value is not null && value < 0
                ? throw new ArticleNegativePropertyException()
                : value;
        }

        public Temperature? DefaultTemperature
        {
            get;
            set;
        }
    }
}
