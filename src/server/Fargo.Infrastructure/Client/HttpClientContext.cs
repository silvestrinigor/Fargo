using Fargo.Application.Common;
using Fargo.Core.Common;
using Fargo.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Fargo.Infrastructure.Client;

public sealed class HttpClientContext(IHttpContextAccessor httpContextAccessor) : IClientContext
{
    public string IpAddress =>
        httpContextAccessor.HttpContext?
        .Connection
        .RemoteIpAddress?
        .ToString()
        ?? throw new FargoInfrastructureException("Could not resolve the client ip address.", FargoErrorType.InvalidOperation);
}
