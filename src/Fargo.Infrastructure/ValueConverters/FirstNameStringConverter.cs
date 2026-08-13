using Fargo.Core.Informations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public class FirstNameStringConverter()
    : ValueConverter<FirstName, string>(x => x.Value, x => new FirstName(x));
