namespace Fargo.Sdk;

/// <summary>
/// Describes a failure returned by the Fargo API.
/// </summary>
public sealed class FargoSdkError
{
    internal FargoSdkError(FargoSdkErrorType type, string detail)
    {
        Type = type;
        Detail = detail;
    }

    /// <summary>Gets the category of the error.</summary>
    public FargoSdkErrorType Type { get; }

    /// <summary>Gets a human-readable description of the error.</summary>
    public string Detail { get; }
}
