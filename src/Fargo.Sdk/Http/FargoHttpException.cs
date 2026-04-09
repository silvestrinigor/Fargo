namespace Fargo.Sdk.Http;

public class FargoHttpException : FargoSdkException
{
    public int StatusCode { get; }

    public string? ResponseBody { get; }

    public FargoHttpException(int statusCode, string? responseBody = null)
        : base(string.IsNullOrWhiteSpace(responseBody)
            ? $"HTTP error {statusCode}."
            : $"HTTP error {statusCode}: {responseBody}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

public sealed class FargoUnauthorizedException() : FargoHttpException(401);

public sealed class FargoNotFoundException() : FargoHttpException(404);
