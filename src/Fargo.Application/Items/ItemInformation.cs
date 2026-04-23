namespace Fargo.Application.Items;

public sealed record ItemInformation(
    Guid Guid,
    Guid ArticleGuid,
    Guid? EditedByGuid = null
);
