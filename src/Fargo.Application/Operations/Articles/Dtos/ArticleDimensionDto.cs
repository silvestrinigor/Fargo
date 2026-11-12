using UnitsNet;

namespace Fargo.Application.Operations.Articles.Dtos;

public sealed record ArticleDimensionDto(
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null
);
