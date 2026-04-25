namespace Fargo.Sdk.ApiClients;

public sealed class ApiClientUpdatedEventArgs(Guid guid) : EventArgs
{
    public Guid Guid { get; } = guid;
}
