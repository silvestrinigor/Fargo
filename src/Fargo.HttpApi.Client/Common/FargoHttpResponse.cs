namespace Fargo.HttpApi.Client.Common;

public sealed record FargoHttpResponse<TValue>(TValue? Value, ProblemDetails? ProblemDetails, HttpResponseMessage HttpResponseMessage)
{
    public bool IsSuccess => HttpResponseMessage.IsSuccessStatusCode;
}

public sealed record FargoHttpResponse(ProblemDetails? ProblemDetails, HttpResponseMessage HttpResponseMessage)
{
    public bool IsSuccess =>
        HttpResponseMessage.IsSuccessStatusCode;
}
