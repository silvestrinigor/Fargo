namespace Fargo.Application.Models.UserModels
{
    public sealed record UserResponseModel(
            Guid Guid,
            string Name,
            string Description
            );
}