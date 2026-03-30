namespace Fargo.Sdk.Exceptions;

public abstract class FargoSdkException : Exception
{
    public string Code { get; }

    public FargoSdkException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}
