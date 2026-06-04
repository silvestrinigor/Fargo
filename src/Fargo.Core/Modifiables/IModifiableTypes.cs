namespace Fargo.Core.Modifiables;

public interface IModifiableTypes<TModificationType> where TModificationType : Enum
{
    IReadOnlySet<TModificationType> GetModificationTypes();

    TModificationType ModificationTypes { get; }
}
