namespace Fargo.Core;

public class FargoException : Exception
{
    public FargoException(string? message) : base(message) { }

    public FargoException(string? message, Exception innerException) : base(message, innerException) { }
}
