using Fargo.Core.Identity;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(FargoDbContext context) : IRefreshTokenRepository
{
    private readonly DbSet<RefreshToken> refreshTokens = context.RefreshTokens;

    public void Add(RefreshToken refreshToken)
    {
        refreshTokens.Add(refreshToken);
    }

    public async Task<RefreshToken?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
    {
        return await refreshTokens.Where(r => r.Guid == entityGuid).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(TokenHash tokenHash, CancellationToken cancellationToken = default)
    {
        return await refreshTokens.Where(r => r.TokenHash == tokenHash).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RefreshToken>> GetByUserGuidAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        return await refreshTokens
            .Where(r => r.UserGuid == userGuid)
            .ToListAsync(cancellationToken);
    }

    public void Remove(RefreshToken refreshToken)
    {
        refreshTokens.Remove(refreshToken);
    }
}
