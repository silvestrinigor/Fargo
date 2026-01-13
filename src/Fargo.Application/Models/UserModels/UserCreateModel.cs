using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public record UserCreateModel(
        int Id,
        Name Name,
        Password Password,
        Description Description);
}
