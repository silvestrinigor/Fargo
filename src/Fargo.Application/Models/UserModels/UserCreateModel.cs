using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public record UserCreateModel(
        Name Name,
        Description? Description = null);
}
