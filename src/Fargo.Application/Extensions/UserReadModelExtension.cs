using Fargo.Application.Models.UserModels;

namespace Fargo.Application.Extensions
{
    public static class UserReadModelExtension
    {
        extension(UserReadModel userReadModel)
        {
            public UserResponseModel ToResponse()
            {
                var userResponseModel = new UserResponseModel(
                        userReadModel.Nameid,
                        userReadModel.Description,
                        [.. userReadModel.UserPermissions.Select(x => x.Action)]
                        );

                return userResponseModel;
            }
        }
    }
}