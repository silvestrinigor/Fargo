namespace Fargo.Application.Shared.Items;

public sealed record ItemCreateDto(
    Guid ArticleGuid,
    IReadOnlyCollection<Guid>? Partitions = null);
