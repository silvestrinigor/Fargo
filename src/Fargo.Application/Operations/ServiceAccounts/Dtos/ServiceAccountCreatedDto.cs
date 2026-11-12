namespace Fargo.Application.Operations.ServiceAccounts.Dtos;

/// <summary>
/// Result of creating a service account or rotating its secret. The plaintext
/// <see cref="ClientSecret"/> is returned exactly once and cannot be retrieved again.
/// </summary>
public sealed record ServiceAccountCreatedDto(
    Guid Guid,
    string ClientId,
    string ClientSecret
);
