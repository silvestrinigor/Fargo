namespace Fargo.Application.Solicitations.Responses
{
    public sealed record ArticleInformation(
        Guid Guid,
        string? Name,
        string? Description,
        DateTime CreatedAt,
        Guid? Parent
    );
}
