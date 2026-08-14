using UnitsNet;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the dimensional changes to apply to an article.
/// </summary>
/// <param name="LengthX">
/// The new length along the X axis. A <see langword="null"/> value leaves the
/// existing value unchanged.
/// </param>
/// <param name="RemoveLengthX">
/// Indicates whether the existing X-axis length should be removed by setting it
/// to <see langword="null"/>. When <see langword="true"/>, this takes precedence
/// over <paramref name="LengthX"/>.
/// </param>
/// <param name="LengthY">
/// The new length along the Y axis. A <see langword="null"/> value leaves the
/// existing value unchanged.
/// </param>
/// <param name="RemoveLengthY">
/// Indicates whether the existing Y-axis length should be removed by setting it
/// to <see langword="null"/>. When <see langword="true"/>, this takes precedence
/// over <paramref name="LengthY"/>.
/// </param>
/// <param name="LengthZ">
/// The new length along the Z axis. A <see langword="null"/> value leaves the
/// existing value unchanged.
/// </param>
/// <param name="RemoveLengthZ">
/// Indicates whether the existing Z-axis length should be removed by setting it
/// to <see langword="null"/>. When <see langword="true"/>, this takes precedence
/// over <paramref name="LengthZ"/>.
/// </param>
public sealed record ArticleDimensionUpdateDto(
    Length? LengthX = null,
    bool RemoveLengthX = false,
    Length? LengthY = null,
    bool RemoveLengthY = false,
    Length? LengthZ = null,
    bool RemoveLengthZ = false
);
