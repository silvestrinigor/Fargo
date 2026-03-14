namespace Fargo.Domain.ValueObjects.Entities
{
    public sealed record ArticleInformation(
        Guid Guid,
        Name Name,
        Description Description
    );
}