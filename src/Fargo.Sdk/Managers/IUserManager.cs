namespace Fargo.Sdk.Managers;

public interface IUserManager
{
    Task CreateUser(string nameid);

    Task DeleteUser(Guid userGuid);
}
