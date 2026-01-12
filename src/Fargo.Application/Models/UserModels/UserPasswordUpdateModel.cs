using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public sealed record UserPasswordUpdateModel(
        Password NewPassword,
        Password CurrentPassword);
}
