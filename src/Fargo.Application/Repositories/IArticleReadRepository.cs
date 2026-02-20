using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;

namespace Fargo.Application.Repositories
{
    public interface IArticleReadRepository
    {
            Task<ArticleReadModel?> GetByGuid(
                    Guid entityGuid,
                    IEnumerable<Guid> partitionGuids,
                    DateTime? asOfDateTime = null,
                    CancellationToken cancellationToken = default
                    );

            Task<IReadOnlyCollection<ArticleReadModel>> GetMany(
                    IEnumerable<Guid> partitionGuids,
                    DateTime? asOfDateTime = null,
                    Pagination pagination = default,
                    CancellationToken cancellationToken = default
                    );
    }
}