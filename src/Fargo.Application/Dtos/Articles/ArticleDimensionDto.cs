using UnitsNet;

namespace Fargo.Application.Dtos.Articles;

public sealed record ArticleDimensionDto(
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null
);
