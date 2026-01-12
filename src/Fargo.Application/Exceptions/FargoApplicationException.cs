namespace Fargo.Application.Exceptions
{
    public class FargoApplicationException : Exception
    {
        public FargoApplicationException() : base() { }

        public FargoApplicationException(string message) : base(message) { }

        public FargoApplicationException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
