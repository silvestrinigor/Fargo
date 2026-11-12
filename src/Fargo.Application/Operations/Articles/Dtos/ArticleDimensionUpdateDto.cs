using UnitsNet;

namespace Fargo.Application.Operations.Articles.Dtos;

/// <summary>
/// For each axis, <see langword="null"/> leaves the length unchanged and the matching
/// <c>Remove*</c> flag clears it. The flag takes precedence over the new length.
/// </summary>
public sealed record ArticleDimensionUpdateDto(
    Length? LengthX = null,
    bool RemoveLengthX = false,
    Length? LengthY = null,
    bool RemoveLengthY = false,
    Length? LengthZ = null,
    bool RemoveLengthZ = false
);
