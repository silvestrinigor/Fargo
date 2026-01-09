using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.UserDtos
{
    public sealed record UserDto(
        Guid Guid,
        Name Name,
        Description Description
        );
}
