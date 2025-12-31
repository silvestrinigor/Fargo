namespace Fargo.Application.Solicitations.Responses
{
    public sealed record ContainerInformation(
        Guid Guid,
        string? Name,
        string? Description,
        DateTime CreatedAt,
        Guid? Parent
    );
}
