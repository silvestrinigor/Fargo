using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public partial class Article
    {
        public bool IsContainer { get; init; } = false;

        public Mass? ContainerMassCapacity
        {
            get;
            init
            {
                ValidadeIsContainer();
                field = value;
            }
        }

        public Volume? ContainerVolumeCapacity
        {
            get;
            init
            {
                ValidadeIsContainer();
                field = value;
            }
        }

        public Temperature? ContainerTemperature
        {
            get;
            init
            {
                ValidadeIsContainer();
                field = value;
            }
        }

        private void ValidadeIsContainer()
        {
            if (!IsContainer)
            {
                throw new InvalidOperationException("error");
            }
        }
    }
}
