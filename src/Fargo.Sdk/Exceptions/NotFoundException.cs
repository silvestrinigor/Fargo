namespace Fargo.Sdk.Exceptions;

public sealed class NotFoundException : FargoApiException
{
    public NotFoundException()
        : base(404, "The requested resource was not found.")
    {
    }
}
