namespace Fargo.Core;

public interface IModifiedEntityTypes<TModificationType> where TModificationType : Enum
{
    IReadOnlySet<TModificationType> GetModificationTypes();

    TModificationType ModificationTypes { get; }
}
