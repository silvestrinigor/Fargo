using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Infrastructure.Persistence.Converter
{
    public sealed class MassConverter : ValueConverter<Mass?, (decimal? Value, string Unit)>
    {
        public MassConverter()
            : base(
                m => m is null
                    ? default
                    : (m.Value, m.Unit.ToString()),
                v => v.Value is null
                    ? null
                    : new Mass(v.Value.Value, Enum.Parse<MassUnit>(v.Unit)))
        { }
    }
}
