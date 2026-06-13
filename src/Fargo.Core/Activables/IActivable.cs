using Fargo.Core.Entities;

namespace Fargo.Core.Activables;

/// <summary>
/// Represents an entity that can be activated or deactivated.
/// </summary>
/// <remarks>
/// Activable entities can participate in business rules that require the entity
/// to be active before an operation is allowed.
/// </remarks>
public interface IActivable : IEntity
{
    /// <summary>
    /// Gets or sets the value indicating whether the entity is currently active.
    /// </summary>
    bool IsActive { get; set; }
}
