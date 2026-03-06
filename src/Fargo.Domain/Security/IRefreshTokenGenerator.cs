using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Security
{
    public interface IRefreshTokenGenerator
    {
        public Token Generate();
    }
}