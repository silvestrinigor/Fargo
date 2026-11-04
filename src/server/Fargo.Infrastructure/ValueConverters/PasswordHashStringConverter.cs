using Fargo.Core.Identity.Credentials;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public class PasswordHashStringConverter()
    : ValueConverter<PasswordHash, string>(x => x.Value, x => new(x));
