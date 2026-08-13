using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleDimensionUpdateDto(
    Length? LengthX = null,
    bool? RemoveLengthX = null,
    Length? LengthY = null,
    bool? RemoveLengthY = null,
    Length? LengthZ = null,
    bool? RemoveLengthZ = null
);
