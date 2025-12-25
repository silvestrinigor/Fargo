namespace Fargo.Domain.Exceptions
{
    public class FargoException : Exception
    {
        public FargoException() { }

        public FargoException(string message) : base(message) { }
    }
}
