using Fargo.Application.Models.ArticleModels;

namespace Fargo.Application.Repositories
{
    public interface IArticleReadRepository
        : IEntityByGuidTemporalPartitionedReadRepository<ArticleReadModel>;
}