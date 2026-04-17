using Fargo.Domain.Tokens;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class TokenHashStringConverter()
    : ValueConverter<TokenHash, string>(x => x.Value, x => new(x));
