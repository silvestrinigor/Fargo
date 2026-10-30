namespace Fargo.Application.Common;

/// <summary>The direction a sort is applied in.</summary>
public enum SortDirection
{
    /// <summary>Ascending order (A-Z, 0-9, oldest first).</summary>
    Ascending = 0,

    /// <summary>Descending order (Z-A, 9-0, newest first).</summary>
    Descending = 1,
}