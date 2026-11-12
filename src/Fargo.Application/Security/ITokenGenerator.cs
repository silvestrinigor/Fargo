using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;

namespace Fargo.Application.Security;

public interface ITokenGenerator
{
    TokenGenerateResult Generate(User user);

    TokenGenerateResult Generate(ServiceAccount serviceAccount);
}
