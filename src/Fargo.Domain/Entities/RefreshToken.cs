using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public sealed class RefreshToken : Entity
    {
        private const short defaultExpirationDays = 10;

        public static TimeSpan DefaultExpirationTimeSpan
        {
            get;
        } = TimeSpan.FromDays(defaultExpirationDays);

        public required Guid UserGuid
        {
            get;
            init;
        }

        public required TokenHash TokenHash
        {
            get;
            init;
        }

        public DateTimeOffset ExpiresAt
        {
            get;
            init;
        } = DateTimeOffset.UtcNow + DefaultExpirationTimeSpan;

        public TokenHash? ReplacedByTokenHash
        {
            get;
            init;
        } = null;

        public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
    }
}