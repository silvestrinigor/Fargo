namespace Fargo.Domain.ValueObjects
{
    public interface IStringValueObject<T>
        where T : IStringValueObject<T>
    {
        string Value { get; }

        static abstract T FromString(string value);
    }
}
