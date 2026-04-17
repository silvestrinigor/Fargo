using Fargo.Domain.Users;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class NameidStringConverter()
    : ValueConverter<Nameid, string>(x => x.Value, x => new Nameid(x));
