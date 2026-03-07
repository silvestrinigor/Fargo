using Fargo.Application.Models.UserModels;

namespace Fargo.Application.Extensions
{
    /// <summary>
    /// Provides extension members for <see cref="UserReadModel"/>.
    /// </summary>
    public static class UserReadModelExtensions
    {
        extension(UserReadModel user)
        {
            /// <summary>
            /// Converts the current <see cref="UserReadModel"/> instance
            /// into a <see cref="UserResponseModel"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="UserResponseModel"/> containing the user information
            /// and the list of granted permissions.
            /// </returns>
            public UserResponseModel ToResponse()
            {
                return new UserResponseModel(
                    user.Nameid,
                    user.Description,
                    [.. user.UserPermissions.Select(x => x.Action)]
                );
            }
        }
    }
}