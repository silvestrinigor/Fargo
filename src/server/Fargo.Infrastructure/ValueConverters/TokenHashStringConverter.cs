using Fargo.Core.Identity.Authentication;
using Fargo.Core.Identity.Tokens;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public class TokenHashStringConverter()
    : ValueConverter<TokenHash, string>(x => x.Value, x => new(x));
