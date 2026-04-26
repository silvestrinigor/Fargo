namespace Fargo.Sdk.Users;

/// <summary>
/// Combined user interface: CRUD and Created events.
/// Inject this when you need everything, or inject the narrower interfaces individually.
/// </summary>
public interface IUserManager : IUserService, IUserEventSource { }
