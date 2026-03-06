using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Security
{
    public interface ITokenHasher
    {
        TokenHash Hash(Token token);
    }
}