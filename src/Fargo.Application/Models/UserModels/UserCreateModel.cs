namespace Fargo.Application.Models.UserModels
{
    public record UserCreateModel(
        int Id,
        string Name,
        string Password,
        string Description = "");
}
