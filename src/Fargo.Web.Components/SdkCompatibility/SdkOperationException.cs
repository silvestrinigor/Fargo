namespace Fargo.Api;

public sealed class InvalidCredentialsFargoSdkException : Exception
{
    public InvalidCredentialsFargoSdkException(string? message = null)
        : base(message ?? "Invalid credentials.")
    {
    }
}

public sealed class PasswordChangeRequiredFargoSdkException : Exception
{
    public PasswordChangeRequiredFargoSdkException(string? message = null)
        : base(message ?? "Password change is required.")
    {
    }
}

internal sealed class SdkOperationException : Exception
{
    public SdkOperationException(FargoSdkError? error, string fallback)
        : base(error?.Detail ?? error?.Title ?? fallback)
    {
        Error = error;
    }

    public FargoSdkError? Error { get; }
}

internal static class SdkResponseExtensions
{
    public static T Unwrap<T>(this FargoSdkResponse<T> response, string fallback)
    {
        if (response.IsSuccess)
        {
            return response.Data!;
        }

        throw new SdkOperationException(response.Error, fallback);
    }

    public static void EnsureSuccess<T>(this FargoSdkResponse<T> response, string fallback)
    {
        if (!response.IsSuccess)
        {
            throw new SdkOperationException(response.Error, fallback);
        }
    }
}
