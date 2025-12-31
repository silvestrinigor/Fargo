namespace Fargo.Application.Solicitations.Responses
{
    public sealed record ItemInformation(
        Guid Guid,
        string? Name,
        string? Description,
        DateTime CreatedAt,
        Guid? Parent
    );
}
