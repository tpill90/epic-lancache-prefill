namespace EpicPrefill.Models.Exceptions
{
    public class ManifestException : Exception
    {
        public ManifestException(string message) : base(message)
        {

        }

        public ManifestException(string message, Exception inner) : base(message, inner)
        {

        }

        public ManifestException()
        {
        }
    }
}