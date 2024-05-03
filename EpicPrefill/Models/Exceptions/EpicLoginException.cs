namespace EpicPrefill.Models.Exceptions
{
    public sealed class EpicLoginException : Exception
    {
        public EpicLoginException(string message) : base(message)
        {

        }

        public EpicLoginException()
        {
        }

        public EpicLoginException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}