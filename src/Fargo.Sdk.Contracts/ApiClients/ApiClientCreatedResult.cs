namespace Fargo.Sdk.Contracts.ApiClients;

/// <summary>
/// Returned from API client creation and contains the generated plain-text key.
/// </summary>
/// <param name="Guid">The unique identifier assigned to the API client.</param>
/// <param name="PlainKey">The plain-text API key, shown only at creation time.</param>
public sealed record ApiClientCreatedResult(Guid Guid, string PlainKey);
