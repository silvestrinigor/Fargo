namespace Fargo.Application.Dtos
{
    public sealed record OptionalSetDto<T>(
        bool SetValue,
        T Value
        );
}
