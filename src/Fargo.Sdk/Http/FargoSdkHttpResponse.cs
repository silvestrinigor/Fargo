using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Fargo.Sdk.Http;

public sealed record FargoSdkHttpResponse<TData>(
    bool IsSuccess,
    TData? Data,
    ProblemDetails? Problem,
    HttpStatusCode StatusCode
);