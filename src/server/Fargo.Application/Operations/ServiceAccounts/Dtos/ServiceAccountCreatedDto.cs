namespace Fargo.Application.Operations.ServiceAccounts.Dtos;

/// <summary>
/// Result of creating (or rotating the secret of) a service account. The
/// plaintext <see cref="ClientSecret"/> is returned exactly once and is never
/// retrievable again.
/// </summary>
public sealed record ServiceAccountCreatedDto(
    Guid Guid,
    string ClientId,
    string ClientSecret
);
