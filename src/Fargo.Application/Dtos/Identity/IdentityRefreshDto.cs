using Fargo.Core.Identity.Tokens;

namespace Fargo.Application.Dtos.Identity;

public sealed record IdentityRefreshDto(Token RefreshToken);
