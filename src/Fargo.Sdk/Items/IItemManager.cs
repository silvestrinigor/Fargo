namespace Fargo.Sdk.Items;

/// <summary>
/// Combined item interface: CRUD and Created events.
/// Inject this when you need everything, or inject the narrower interfaces individually.
/// </summary>
public interface IItemManager : IItemService, IItemEventSource { }
