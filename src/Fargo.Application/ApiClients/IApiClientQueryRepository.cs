using Fargo.Domain;

namespace Fargo.Application.ApiClients;

public interface IApiClientQueryRepository
{
    Task<ApiClientInformation?> GetInfoByGuid(Guid guid, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ApiClientInformation>> GetManyInfo(Pagination? pagination = null, string? search = null, CancellationToken cancellationToken = default);
    Task<Guid?> FindActiveGuidByKeyHash(string keyHash, CancellationToken cancellationToken = default);
}
