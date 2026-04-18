using Fargo.Domain;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the data used to update an existing article.
/// </summary>
/// <remarks>
/// All properties are optional. Only the provided values will be updated.
/// </remarks>
/// <param name="Name">
/// The new name of the article. If null, the name will not be changed.
/// </param>
/// <param name="Description">
/// The new description of the article. If null, the description will not be changed.
/// </param>
public sealed record ArticleUpdateModel(
        Name? Name = null,
        Description? Description = null,
        Mass? Mass = null,
        Length? LengthX = null,
        Length? LengthY = null,
        Length? LengthZ = null
        );
