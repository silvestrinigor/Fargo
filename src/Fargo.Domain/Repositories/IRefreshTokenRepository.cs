using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                );

        Task<RefreshToken?> GetByTokenHash(
                TokenHash tokenHash,
                CancellationToken cancellationToken = default
                );

        void Add(RefreshToken refreshToken);

        void Remove(RefreshToken refreshToken);
    }
}