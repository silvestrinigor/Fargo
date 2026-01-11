using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.UserDtos
{
    public record UserCreateDto(
        Name Name,
        Description? Description = null);
}
