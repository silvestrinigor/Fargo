// IClientSessionStore.cs
namespace Fargo.Web.Security;

public interface IClientSessionStore
{
    Task SaveAsync(AuthenticatedSession session, bool rememberMe);
    Task<AuthenticatedSession?> GetAsync();
    Task ClearAsync();
}
