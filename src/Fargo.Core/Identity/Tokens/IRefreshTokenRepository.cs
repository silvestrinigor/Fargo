namespace Fargo.Core.Identity.Tokens;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashAsync(TokenHash tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RefreshToken>> GetByUserGuidAsync(Guid userGuid, CancellationToken cancellationToken = default);

    void Add(RefreshToken refreshToken);

    void Remove(RefreshToken refreshToken);
}
