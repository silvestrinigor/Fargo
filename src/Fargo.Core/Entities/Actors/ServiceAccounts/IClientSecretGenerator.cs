using Fargo.Core.Identity.Credentials;

namespace Fargo.Core.Entities.Actors.ServiceAccounts;

/// <summary>
/// Generates client secrets that satisfy the <see cref="Password"/> policy. The plaintext is shown
/// once (at create / rotate time); only its hash is persisted.
/// </summary>
public interface IClientSecretGenerator
{
    Password Generate();
}
