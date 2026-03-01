using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.UserServices
{
    public class UserDeleteService(
            IUserRepository userRepository
            )
    {
        public void DeleteUser(
                User actor,
                User userToDelete
                )
        {
            actor.ValidatePermission(ActionType.DeleteUser);

            userRepository.Remove(userToDelete);
        }
    }
}