namespace Fargo.Sdk.Exceptions;

public abstract class FargoSdkException : Exception
{
    internal FargoSdkException(string message)
        : base(message)
    {

    }
}
