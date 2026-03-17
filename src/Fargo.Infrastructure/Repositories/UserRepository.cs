using Fargo.Application.Mappings;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserRepository(FargoDbContext context) : IUserRepository
{
    private readonly DbSet<User> users = context.Users;

    public Task<bool> Any(CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(cancellationToken);
    }

    public void Add(User user)
    {
        context.Users.Add(user);
    }

    public async Task<User?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default)
    {
        return await users
            .Include(user => user.Permissions)
            .Include(user => user.UserGroups)
            .Where(user => user.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        return await users
            .Where(user => user.Nameid == nameid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
    }

    public async Task<bool> ExistsByGuid(
        Guid guid,
        CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(user => user.Guid == guid, cancellationToken);
    }

    public async Task<bool> ExistsByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(user => user.Nameid == nameid, cancellationToken);
    }

    public async Task<UserInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await users
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(user => user.Guid == entityGuid)
            .Select(UserMappings.InformationProjection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserInformation>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await users
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .OrderBy(user => user.Guid)
            .WithPagination(pagination)
            .Select(UserMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }
}
