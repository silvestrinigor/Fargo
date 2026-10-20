using Fargo.ClientHttp.Models;
using Microsoft.Kiota.Abstractions;

namespace Fargo.ClientHttp.AuditLogs.Item;

/// <summary>
/// Builds and executes requests for operations under
/// <c>\audit-logs\{auditLogGuid}</c>.
/// </summary>
public sealed class WithAuditLogGuidItemRequestBuilder : BaseRequestBuilder
{
    public WithAuditLogGuidItemRequestBuilder(
        Dictionary<string, object> pathParameters,
        IRequestAdapter requestAdapter)
        : base(requestAdapter, "{+baseurl}/audit-logs/{auditLogGuid}", pathParameters)
    {
    }

    public WithAuditLogGuidItemRequestBuilder(
        string rawUrl,
        IRequestAdapter requestAdapter)
        : base(requestAdapter, "{+baseurl}/audit-logs/{auditLogGuid}", rawUrl)
    {
    }

    /// <summary>
    /// Retrieves a single audit log by its unique identifier, including its
    /// metadata.
    /// </summary>
    /// <param name="requestConfiguration">Optional request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<AuditLogDto?> GetAsync(
        Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default,
        CancellationToken cancellationToken = default)
    {
        var requestInfo = ToGetRequestInformation(requestConfiguration);

        return await RequestAdapter.SendAsync<AuditLogDto>(
            requestInfo,
            AuditLogDto.CreateFromDiscriminatorValue,
            default,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Builds the request information for the GET operation.</summary>
    public RequestInformation ToGetRequestInformation(
        Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default)
    {
        var requestInfo = new RequestInformation(Method.GET, UrlTemplate, PathParameters);
        requestInfo.Configure(requestConfiguration);
        requestInfo.Headers.TryAdd("Accept", "application/json");
        return requestInfo;
    }
}
