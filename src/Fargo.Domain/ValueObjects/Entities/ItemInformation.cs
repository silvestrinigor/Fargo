namespace Fargo.Domain.ValueObjects.Entities
{
    public sealed record ItemInformation(
            Guid Guid,
            Guid ArticleGuid
            );
}