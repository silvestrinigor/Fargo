using Fargo.Domain;

namespace Fargo.Application.Articles;

public sealed record ArticleInformation(
    Guid Guid,
    Name Name,
    Description Description,
    Mass? Mass,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null,
    bool HasImage = false,
    Guid? EditedByGuid = null
);
