using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public sealed record UserUpdateModel(
        Name? Name,
        Description? Description);
}