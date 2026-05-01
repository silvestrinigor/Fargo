using System.Net;

namespace Fargo.Api.Http;

public sealed record FargoSdkHttpResponse<TData>(
    bool IsSuccess,
    TData? Data,
    FargoProblemDetails? Problem,
    HttpStatusCode StatusCode
);
