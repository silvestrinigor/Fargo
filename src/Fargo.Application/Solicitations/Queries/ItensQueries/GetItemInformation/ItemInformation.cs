namespace Fargo.Application.Solicitations.Queries.ItensQueries.GetItemInformation
{
    public sealed record ItemInformation(
        Guid Guid,
        string? Name,
        string? Description,
        DateTime CreatedAt
    );
}
