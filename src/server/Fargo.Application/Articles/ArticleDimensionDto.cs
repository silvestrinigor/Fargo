using UnitsNet;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the physical dimensions of an article.
/// </summary>
/// <param name="LengthX">The optional length along the X axis.</param>
/// <param name="LengthY">The optional length along the Y axis.</param>
/// <param name="LengthZ">The optional length along the Z axis.</param>
public sealed record ArticleDimensionDto(
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null
);
