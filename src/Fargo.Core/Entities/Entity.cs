namespace Fargo.Core.Entities;

/// <summary>
/// Base class for domain entities identified by a <see cref="Guid"/>.
///
/// Implements identity-based equality comparison, meaning two entities
/// are considered equal when:
/// - They are of the same concrete type
/// - Their Guid identifiers are equal
/// - The identifier is not <see cref="Guid.Empty"/>
///
/// This class also overloads equality operators (== and !=)
/// to provide value semantics based on identity.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    /// <remarks>
    /// A new identifier is generated automatically using <see cref="Guid.NewGuid"/>
    /// when the entity instance is created.
    ///
    /// The identifier cannot be <see cref="Guid.Empty"/>. Attempting to initialize
    /// this property with an empty value results in an <see cref="ArgumentException"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the assigned value is <see cref="Guid.Empty"/>.
    /// </exception>
    public Guid Guid
    {
        get;
        internal init
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Entity Guid cannot be empty.", nameof(value));
            }

            field = value;
        }
    } = Guid.NewGuid();
}
