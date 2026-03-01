using Fargo.Application.Models.AuthModels;
using Fargo.Domain.Entities;

namespace Fargo.Application.Security
{
    public interface ITokenGenerator
    {
        AuthResult Generate(User user);
    }
}