namespace EpicPrefill.Extensions
{
    public static class MiscExtensions
    {
        public static byte[] Decompress(this byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            using (var gZipStream = new DeflateStream(memoryStream, CompressionMode.Decompress, true))
            using (var memoryStreamOutput = new MemoryStream())
            {
                gZipStream.CopyTo(memoryStreamOutput);
                var outputBytes = memoryStreamOutput.ToArray();

                return outputBytes;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA5350", Justification = "SHA1 is required by Epic")]
        public static byte[] ComputeSha1Hash(this byte[] input)
        {
            return SHA1.HashData(input);
        }

        public static string ToHexString(this byte[] input)
        {
            return HexMate.Convert.ToHexString(input, HexFormattingOptions.Lowercase);
        }

        //TODO see if this can be replaced with String.Format("X8"0)
        public static string ToHexStringUpper(this byte[] input)
        {
            return HexMate.Convert.ToHexString(input);
        }

    }
}