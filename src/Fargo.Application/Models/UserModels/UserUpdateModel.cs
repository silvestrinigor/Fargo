using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public sealed record UserUpdateModel(
            Nameid? Nameid = null,
            Description? Description = null,
            UserPasswordUpdateModel? Password = null
            );
}