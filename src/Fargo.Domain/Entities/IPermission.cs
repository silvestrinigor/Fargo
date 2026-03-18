using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities;

/// <summary>
/// Represents an object that grants a specific permission action.
/// </summary>
/// <remarks>
/// This abstraction allows different permission sources, such as direct user
/// permissions or group permissions, to be evaluated uniformly.
/// </remarks>
public interface IPermission
{
    /// <summary>
    /// Gets the action granted by the permission.
    /// </summary>
    ActionType Action { get; }
}