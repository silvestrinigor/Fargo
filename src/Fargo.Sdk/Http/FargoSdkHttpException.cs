namespace Fargo.Sdk.Http;

public class FargoSdkHttpException : FargoSdkException
{
    public int StatusCode { get; }

    public string? ResponseBody { get; }

    public FargoSdkHttpException(int statusCode, string? responseBody = null)
        : base(string.IsNullOrWhiteSpace(responseBody)
            ? $"HTTP error {statusCode}."
            : $"HTTP error {statusCode}: {responseBody}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

public sealed class FargoUnauthorizedException() : FargoSdkHttpException(401);

public sealed class FargoNotFoundException() : FargoSdkHttpException(404);
