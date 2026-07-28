using Fargo.Core.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class NameidStringConverter()
    : ValueConverter<Nameid, string>(x => x.Value, x => new Nameid(x));
