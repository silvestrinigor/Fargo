using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    public class RefreshTokenRepository(FargoDbContext context) : IRefreshTokenRepository
    {
        private readonly DbSet<RefreshToken> refreshTokens = context.RefreshTokens;

        public void Add(RefreshToken refreshToken)
        {
            refreshTokens.Add(refreshToken);
        }

        public async Task<RefreshToken?> GetByGuid(Guid entityGuid, CancellationToken cancellationToken = default)
        {
            return await refreshTokens.Where(r => r.Guid == entityGuid).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<RefreshToken?> GetByTokenHash(TokenHash tokenHash, CancellationToken cancellationToken = default)
        {
            return await refreshTokens.Where(r => r.TokenHash == tokenHash).SingleOrDefaultAsync(cancellationToken);
        }

        public void Remove(RefreshToken refreshToken)
        {
            refreshTokens.Remove(refreshToken);
        }
    }
}