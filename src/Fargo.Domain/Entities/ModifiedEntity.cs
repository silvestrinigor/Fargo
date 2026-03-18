namespace Fargo.Domain.Entities;

/// <summary>
/// Represents a base entity that tracks last modification metadata.
/// </summary>
/// <remarks>
/// This entity extends <see cref="Entity"/> and implements
/// <see cref="IModifiedEntity"/> to provide information about
/// the last modification performed on the entity.
///
/// The modification metadata is expected to be managed automatically
/// by the application or infrastructure layer (e.g., change tracker).
///
/// <para>
/// <strong>Important:</strong>
/// <see cref="EditedByGuid"/> is expected to be <see langword="null"/>
/// only during the entity creation phase. After the entity is tracked
/// and persisted, this value should always be populated.
/// A <see langword="null"/> value outside this scenario typically
/// indicates a misconfiguration or a missing auditing operation.
/// </para>
/// </remarks>
public abstract class ModifiedEntity : Entity, IModifiedEntity
{
    /// <inheritdoc />
    public Guid? EditedByGuid
    {
        get;
        private set;
    }

    /// <inheritdoc />
    public void MarkAsEdited(Guid userGuid)
    {
        EditedByGuid = userGuid;
    }
}