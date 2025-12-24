using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Domain.Services
{
    public class ContainerService(IContainerRepository containerRepository)
    {
        private readonly IContainerRepository containerRepository = containerRepository;

        public void InsertEntityIntoContainer()
        {

        }
    }
}
