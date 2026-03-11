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
                        user.Guid,
                        user.Nameid,
                        user.FirstName,
                        user.LastName,
                        user.DefaultPasswordExpirationPeriod,
                        user.Description,
                        user.IsActive,
                        [.. user.UserPermissions.Select(x =>
                            new UserPermissionResponseModel(
                                x.Guid,
                                x.Action
                                )
                            )]
                        );
            }
        }
    }
}