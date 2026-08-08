using Fargo.Core.Shared.Informations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public class NameidStringConverter()
    : ValueConverter<Nameid, string>(x => x.Value, x => new Nameid(x));
