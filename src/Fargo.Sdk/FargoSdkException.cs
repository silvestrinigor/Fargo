namespace Fargo.Sdk;

public abstract class FargoSdkException : Exception
{
    protected FargoSdkException()
    {
    }

    protected FargoSdkException(string? message)
        : base(message)
    {
    }

    protected FargoSdkException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
