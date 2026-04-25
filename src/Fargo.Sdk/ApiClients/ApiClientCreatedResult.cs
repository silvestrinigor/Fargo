namespace Fargo.Sdk.ApiClients;

/// <summary>Returned from create — contains the GUID and the plain API key (shown once).</summary>
public sealed record ApiClientCreatedResult(Guid Guid, string PlainKey);
