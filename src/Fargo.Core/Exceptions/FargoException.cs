namespace Fargo.Domain.Exceptions
{
    public class FargoException 
        : Exception
    {
        public FargoException() 
            : base()
        {

        }

        public FargoException(string? message) 
            : base(message)    
        {

        } 

        public FargoException(string? message, Exception? innerException) 
            : base(message, innerException) 
        {

        }
    }
}