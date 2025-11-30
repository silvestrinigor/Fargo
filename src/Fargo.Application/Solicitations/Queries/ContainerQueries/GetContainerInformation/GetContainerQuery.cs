using Fargo.Application.Interfaces.Solicitations.Queries;

namespace Fargo.Application.Solicitations.Queries.ContainerQueries.GetContainerInformation
{
    public sealed record GetContainerQuery(Guid ContainerGuid) : IQuery<GetContainerQuery, Task<ContainerInformation?>>;
}
