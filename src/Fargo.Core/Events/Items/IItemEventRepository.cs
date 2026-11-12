namespace Fargo.Core.Events;

public interface IItemEventRepository
{
    void Add(ItemEvent itemEvent);
}
