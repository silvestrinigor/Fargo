using Fargo.Application.Models.UserGroupModels;

namespace Fargo.Application.Extensions
{
    /// <summary>
    /// Provides extension members for <see cref="UserGroupReadModel"/>.
    /// </summary>
    public static class UserGroupReadModelExtensions
    {
        extension(UserGroupReadModel userGroup)
        {
            /// <summary>
            /// Converts the current <see cref="UserGroupReadModel"/> instance
            /// into a <see cref="UserGroupResponseModel"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="UserGroupResponseModel"/> containing the user group information,
            /// auditing metadata, and the list of granted permissions.
            /// </returns>
            public UserGroupResponseModel ToResponse()
            {
                return new UserGroupResponseModel(
                    userGroup.Guid,
                    userGroup.Nameid,
                    userGroup.Description,
                    userGroup.IsActive,
                    userGroup.CreatedAt,
                    userGroup.CreatedByGuid,
                    userGroup.EditedAt,
                    userGroup.EditedByGuid,
                    [.. userGroup.UserGroupPermissions.Select(x =>
                        new UserGroupPermissionResponseModel(
                            x.Guid,
                            x.Action))]
                );
            }
        }
    }
}