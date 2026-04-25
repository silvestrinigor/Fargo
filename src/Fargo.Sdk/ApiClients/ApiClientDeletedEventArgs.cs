namespace Fargo.Sdk.ApiClients;

public sealed class ApiClientDeletedEventArgs(Guid guid) : EventArgs
{
    public Guid Guid { get; } = guid;
}
