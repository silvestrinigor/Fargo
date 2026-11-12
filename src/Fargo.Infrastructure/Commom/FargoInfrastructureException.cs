using Fargo.Core.Common;

namespace Fargo.Infrastructure.Exceptions;

public class FargoInfrastructureException : Exception
{
    private const string defaultExceptionMessage = "Fargo infrastructure exception.";

    public FargoErrorType ErrorType { get; init; } = FargoErrorType.None;

    public FargoInfrastructureException() : base(defaultExceptionMessage) { }

    public FargoInfrastructureException(string? message) : base(message) { }

    public FargoInfrastructureException(string? message, Exception innerException)
        : base(message, innerException) { }

    public FargoInfrastructureException(string? message, FargoErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    public FargoInfrastructureException(string? message, FargoErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    public FargoInfrastructureException(FargoErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
