namespace Fargo.Application.Solicitations.Queries.ContainerQueries.GetContainerInformation
{
    public sealed record ContainerInformation(
        Guid Guid,
        string? Name,
        string? Description,
        DateTime CreatedAt
    );
}
