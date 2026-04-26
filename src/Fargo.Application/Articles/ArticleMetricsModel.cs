using Fargo.Domain;

namespace Fargo.Application.Articles;

/// <summary>
/// Groups the physical measurement properties used when creating or updating an article.
/// </summary>
/// <param name="Mass">
/// Optional physical mass. When <see langword="null"/>, mass is left unchanged (update) or
/// unset (create).
/// </param>
/// <param name="LengthX">Optional X dimension.</param>
/// <param name="LengthY">Optional Y dimension.</param>
/// <param name="LengthZ">Optional Z dimension.</param>
public sealed record ArticleMetricsModel(
    Mass? Mass = null,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null);
