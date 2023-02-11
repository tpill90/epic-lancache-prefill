namespace EpicPrefill.Extensions
{
    public static class BinaryReaderExtensions
    {
        //TODO document
        public static string ReadGuid(this BinaryReader br, byte[] sharedBuffer)
        {
            br.Read(sharedBuffer);

            //TODO comment what I'm doing here
            sharedBuffer.AsSpan(0, 4).Reverse();
            sharedBuffer.AsSpan(4, 4).Reverse();
            sharedBuffer.AsSpan(8, 4).Reverse();
            sharedBuffer.AsSpan(12, 4).Reverse();
            return sharedBuffer.ToHexStringUpper();
        }
    }
}