using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public partial class Article
    {
        public bool IsContainer { get; init; } = false;
        public Mass? ContainerMassCapacity { get; init; }
        public Volume? ContainerVolumeCapacity { get; init; }
        public Temperature? ContainerTemperature { get; init; }
    }
}
