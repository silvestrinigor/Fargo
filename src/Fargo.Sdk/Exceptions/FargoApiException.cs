namespace Fargo.Sdk.Exceptions;

public class FargoApiException : FargoSdkException
{
    public int StatusCode { get; }

    public FargoApiException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
