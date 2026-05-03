using UnitsNet;

namespace Fargo.Application.Articles;

public sealed record ArticleMetricsDto(
    Mass? Mass = null,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null
);