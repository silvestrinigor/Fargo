using Fargo.Domain.Entities;

namespace Fargo.Domain.Exceptions
{
    public class UserInvalidPasswordException(User user)
        : FargoException
    {
        public User User
        {
            get;
        } = user;
    }
}