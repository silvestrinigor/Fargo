using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleDimensionDto(
    Length? LengthX,
    Length? LengthY,
    Length? LengthZ
);
