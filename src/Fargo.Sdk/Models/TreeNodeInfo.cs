namespace Fargo.Sdk.Models;

public sealed class TreeNodeInfo
{
    public string Nodeid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public bool HasChildren { get; set; }
    public bool IsActive { get; set; }
}
