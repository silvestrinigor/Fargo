using Fargo.Application.Interfaces.Solicitations.Queries;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Solicitations.Queries.ContainerQueries.GetContainerInformation
{
    public class GetContainerHandler(IContainerRepository containerRepository) : IQueryHandler<GetContainerQuery, Task<ContainerInformation?>>
    {
        private readonly IContainerRepository containerRepository = containerRepository;

        public async Task<ContainerInformation?> Handle(GetContainerQuery query)
        {
            var article = await containerRepository.GetByGuidAsync(query.ContainerGuid);

            if (article is null)
            {
                return null;
            }

            return new ContainerInformation(article.Guid, article.Name, article.Description, article.CreatedAt);
        }
    }
}
