namespace EpicPrefill.Extensions
{
    public static class BinaryReaderExtensions
    {
        //TODO document
        [SuppressMessage("Performance", "EPS06:Hidden struct copy operation", Justification = "I don't believe this analyzer is correct.  " +
                                                                                                                " Benchmarks show no additional allocations")]
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