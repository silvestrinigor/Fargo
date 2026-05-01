using Fargo.Application.ApiClients;
using Fargo.Domain;
using Fargo.Domain.ClientApplications;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ApiClientRepository(FargoDbContext context) : IApiClientRepository, IApiClientQueryRepository
{
    private readonly DbSet<ClientApplication> clients = context.ApiClients;

    public void Add(ClientApplication client) => context.ApiClients.Add(client);
    public void Remove(ClientApplication client) => context.ApiClients.Remove(client);

    public async Task<ClientApplication?> GetByGuid(Guid guid, CancellationToken cancellationToken = default)
    {
        return await clients
            .Where(c => c.Guid == guid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByGuid(Guid guid, CancellationToken cancellationToken = default)
    {
        return await clients.AnyAsync(c => c.Guid == guid, cancellationToken);
    }

    public async Task<ApiClientInformation?> GetInfoByGuid(Guid guid, CancellationToken cancellationToken = default)
    {
        return await clients
            .AsNoTracking()
            .Where(c => c.Guid == guid)
            .Select(ApiClientMappings.InformationProjection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ApiClientInformation>> GetManyInfo(Pagination? pagination = null, string? search = null, CancellationToken cancellationToken = default)
    {
        IQueryable<ClientApplication> query = clients.AsNoTracking();

        if (search is not null)
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{search}%"));
        }

        if (pagination is not null)
        {
            query = query.OrderBy(c => c.Guid).WithPagination(pagination.Value);
        }

        return await query
            .Select(ApiClientMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> FindActiveGuidByKeyHash(string keyHash, CancellationToken cancellationToken = default)
    {
        return await clients
            .AsNoTracking()
            .Where(c => c.KeyHash == keyHash && c.IsActive)
            .Select(c => (Guid?)c.Guid)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
