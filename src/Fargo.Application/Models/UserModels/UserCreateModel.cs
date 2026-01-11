namespace Fargo.Application.Models.UserModels
{
    public record UserCreateModel(
        string Name,
        string Password,
        string Description = "");
}
