namespace Fargo.Application.Models.UserModels
{
    public sealed record UserPasswordUpdateModel(
        string NewPassword,
        string CurrentPassword);
}
