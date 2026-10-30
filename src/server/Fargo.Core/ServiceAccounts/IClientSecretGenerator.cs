using Fargo.Core.Security;

namespace Fargo.Core.ServiceAccounts;

/// <summary>
/// Generates cryptographically strong client secrets for service accounts that
/// satisfy the <see cref="Password"/> policy. The plaintext is returned to the
/// caller once (at create / rotate time) and only its hash is persisted.
/// </summary>
public interface IClientSecretGenerator
{
    /// <summary>Generates a new, policy-compliant client secret.</summary>
    Password Generate();
}