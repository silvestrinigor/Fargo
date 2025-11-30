using Fargo.Application.Interfaces.Solicitations.Queries;

namespace Fargo.Application.Solicitations.Queries.ItensQueries.GetItemInformation
{
    public sealed record GetItemQuery(Guid ItemGuid) : IQuery<GetItemQuery, Task<ItemInformation?>>;
}
