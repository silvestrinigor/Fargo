namespace Fargo.Core.Items.Events;

public interface IItemEventRepository
{
    void Add(ItemMovimentEvent itemMovimentEvent);
}
