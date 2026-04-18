using Fargo.Application.Models.TreeModels;
using Fargo.Domain;

namespace Fargo.Application.Repositories;

/// <summary>
/// Provides access methods for retrieving article-related nodes in a tree structure.
/// </summary>
/// <remarks>
/// This repository exposes a hierarchical view of articles and their related items.
///
/// The tree is composed of:
/// <list type="bullet">
/// <item><description>Root-level articles</description></item>
/// <item><description>Items associated with a specific article</description></item>
/// </list>
/// </remarks>
public interface IArticleTreeRepository
{
    /// <summary>
    /// Retrieves a paginated collection of article-related tree nodes.
    /// </summary>
    /// <remarks>
    /// When <paramref name="articleGuid"/> is <c>null</c>, the method returns
    /// root-level article nodes.
    ///
    /// When <paramref name="articleGuid"/> is provided, the method returns
    /// the items associated with the specified article.
    /// </remarks>
    /// <param name="pagination">
    /// The pagination settings used to limit and organize the result set.
    /// </param>
    /// <param name="articleGuid">
    /// The unique identifier of the article.
    /// If <c>null</c>, root-level articles are returned; otherwise, the items
    /// related to the specified article are returned.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="EntityTreeNode"/> representing
    /// either articles or items, depending on the query context.
    /// </returns>
    Task<IReadOnlyCollection<EntityTreeNode>> GetArticleTreeNodes(
        Pagination pagination,
        Guid? articleGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a paginated collection of article-related tree nodes
    /// filtered by a set of partition identifiers.
    /// </summary>
    /// <remarks>
    /// Applies the same behavior as <see cref="GetArticleTreeNodes"/>,
    /// but restricts the results to articles (or their items) that are
    /// associated with the specified partitions.
    /// </remarks>
    /// <param name="pagination">
    /// The pagination settings used to limit and organize the result set.
    /// </param>
    /// <param name="articleGuid">
    /// The unique identifier of the article.
    /// If <c>null</c>, root-level articles are returned; otherwise, the items
    /// related to the specified article are returned.
    /// </param>
    /// <param name="partitionGuids">
    /// The collection of partition identifiers used to filter the results.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="EntityTreeNode"/> representing
    /// filtered article nodes or their related items.
    /// </returns>
    Task<IReadOnlyCollection<EntityTreeNode>> GetArticleTreeNodesInPartitions(
        Pagination pagination,
        Guid? articleGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        CancellationToken cancellationToken = default
    );
}
