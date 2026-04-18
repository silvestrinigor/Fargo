using Fargo.Domain;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the data required to create a new article.
/// </summary>
/// <param name="Name">
/// The name of the article.
/// This value must satisfy the validation rules defined in <see cref="Name"/>.
/// </param>
/// <param name="Description">
/// Optional description associated with the article.
/// When provided, the value must satisfy the validation rules defined in
/// <see cref="Description"/>.
/// </param>
/// <param name="FirstPartition">
/// Optional identifier of the first partition to associate with the article
/// during creation.
/// </param>
/// <remarks>
/// Articles are partitioned entities and may belong to multiple partitions.
/// When <paramref name="FirstPartition"/> is provided, the created article
/// is initially associated with that partition.
/// </remarks>
public record ArticleCreateModel(
    Name Name,
    Description? Description = null,
    Guid? FirstPartition = null,
    Mass? Mass = null,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null);
