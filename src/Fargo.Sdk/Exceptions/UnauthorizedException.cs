namespace Fargo.Sdk.Exceptions;

public sealed class UnauthorizedException : FargoApiException
{
    public UnauthorizedException()
        : base(401, "Unauthorized. Please log in again.")
    {
    }
}
