namespace Fargo.Api.UserGroups;

/// <summary>
/// Combined user group interface: CRUD and Created events.
/// Inject this when you need everything, or inject the narrower interfaces individually.
/// </summary>
public interface IUserGroupManager : IUserGroupService, IUserGroupEventSource { }
