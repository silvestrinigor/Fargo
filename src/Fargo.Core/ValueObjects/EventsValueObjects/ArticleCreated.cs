namespace Fargo.Domain.ValueObjects.EventsValueObjects
{
    public sealed record ArticleCreated(
        Name Name,
        Description Description
        );
}