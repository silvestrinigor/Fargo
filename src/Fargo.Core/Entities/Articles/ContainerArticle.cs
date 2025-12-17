using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public partial class ContainerArticle
    {
        public required Article Article { get; init; }

        public Mass? MassCapacity { get; init; }

        public Volume? VolumeCapacity { get; init; }

        public Temperature? Temperature { get; init; }
    }
}
