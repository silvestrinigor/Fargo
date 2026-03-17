using Fargo.Application.Models.ArticleModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.HttpApi.Client.Contracts;

public interface IArticleClient
{
    Task<ArticleInformation?> GetSingleAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ArticleInformation>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        ArticleCreateModel model,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid articleGuid,
        ArticleUpdateModel model,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);
}
