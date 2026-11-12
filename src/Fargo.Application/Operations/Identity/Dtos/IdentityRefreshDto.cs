using Fargo.Core.Identity.Tokens;

namespace Fargo.Application.Operations.Identity.Dtos;

public sealed record IdentityRefreshDto(Token RefreshToken);
