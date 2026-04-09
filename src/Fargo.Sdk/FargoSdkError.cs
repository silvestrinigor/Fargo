namespace Fargo.Sdk;

public sealed class FargoSdkError
{
    internal FargoSdkError(FargoSdkErrorType type, string detail)
    {
        Type = type;

        Detail = detail;
    }

    public FargoSdkErrorType Type { get; }

    public string Detail { get; }
}
