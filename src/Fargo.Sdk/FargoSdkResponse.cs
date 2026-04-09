namespace Fargo.Sdk;

public sealed class FargoSdkResponse<TData>
{
    internal FargoSdkResponse()
    {
    }

    internal FargoSdkResponse(TData data)
    {
        Data = data;
    }

    internal FargoSdkResponse(FargoSdkError error)
    {
        Error = error;
    }

    public bool IsSuccess => Error is null;

    public TData? Data { get; }

    public FargoSdkError? Error { get; }
}