using Fargo.Application.Dtos;
using Fargo.Domain.Entities;

namespace Fargo.Application.Extensions
{
    public static class UserExtension
    {
        extension(User user)
        {
            public UserDto ToDto()
            {
                return new UserDto(
                    user.Guid,
                    user.Name,
                    user.Description
                    );
            }
        }
    }
}
