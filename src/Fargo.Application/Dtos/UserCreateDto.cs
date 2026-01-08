
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos
{
    public record UserCreateDto(
        Name Name,
        Description Description
        );
}
