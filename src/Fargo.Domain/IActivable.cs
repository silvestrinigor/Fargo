namespace Fargo.Domain;

/// <summary>
/// Represents an entity that can be activated or deactivated.
/// </summary>
/// <remarks>
/// Implementing types should define how activation state is managed,
/// typically controlling availability or lifecycle behavior
/// within the domain.
/// </remarks>
public interface IActivable
{
    /// <summary>
    /// Gets a value indicating whether the entity is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Activates the entity.
    /// </summary>
    /// <remarks>
    /// Implementations should set <see cref="IsActive"/> to <c>true</c>
    /// and apply any domain rules associated with activation.
    /// </remarks>
    void Activate();

    /// <summary>
    /// Deactivates the entity.
    /// </summary>
    /// <remarks>
    /// Implementations should set <see cref="IsActive"/> to <c>false</c>
    /// and enforce any domain rules associated with deactivation.
    /// </remarks>
    void Deactivate();
}
