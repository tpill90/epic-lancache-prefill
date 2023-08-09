namespace EpicPrefill.Models.Exceptions
{
    public class EpicLoginException : Exception
    {
        protected EpicLoginException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public EpicLoginException()
        {

        }

        public EpicLoginException(string message) : base(message)
        {

        }

        public EpicLoginException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}