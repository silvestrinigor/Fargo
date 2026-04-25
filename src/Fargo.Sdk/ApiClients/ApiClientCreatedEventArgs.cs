namespace Fargo.Sdk.ApiClients;

public sealed class ApiClientCreatedEventArgs(Guid guid) : EventArgs
{
    public Guid Guid { get; } = guid;
}
