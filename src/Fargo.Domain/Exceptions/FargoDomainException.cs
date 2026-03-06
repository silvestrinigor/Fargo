namespace Fargo.Domain.Exceptions
{
    public class FargoDomainException
        : Exception
    {
        public FargoDomainException()
            : base()
        {

        }

        public FargoDomainException(string? message)
            : base(message)
        {

        }

        public FargoDomainException(string? message, Exception? innerException)
            : base(message, innerException)
        {

        }
    }
}