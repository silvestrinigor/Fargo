namespace Fargo.Application.Dtos;

/// <summary>
/// Request body of the dedicated activate/deactivate endpoints.
/// </summary>
public sealed record ActiveStateDto(bool IsActive);
