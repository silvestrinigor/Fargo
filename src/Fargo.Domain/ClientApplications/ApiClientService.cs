namespace Fargo.Domain.ClientApplications;

/// <summary>
/// Well-known GUIDs for the system-seeded API clients.
/// </summary>
public static class ClientApplicationService
{
    /// <summary>
    /// Fixed GUID for the Fargo Web client.
    /// </summary>
    public static readonly Guid WebApiClientGuid = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    /// <summary>
    /// Fixed GUID for the Fargo MCP client.
    /// </summary>
    public static readonly Guid McpApiClientGuid = new("b2c3d4e5-f6a7-8901-bcde-f12345678901");

    /// <summary>
    /// Fixed GUID for the development Test client.
    /// </summary>
    public static readonly Guid TestApiClientGuid = new("c3d4e5f6-a7b8-9012-cdef-123456789012");
}
