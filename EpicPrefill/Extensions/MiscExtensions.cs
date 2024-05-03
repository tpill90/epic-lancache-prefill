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

        //TODO document + rename
        public static string ReadFstring(this BinaryReader br)
        {
            var stringLength = br.ReadInt32();

            // If the length is negative the string is UTF-16 encoded
            if (stringLength < 0)
            {
                // utf-16 chars are (generally) 2 bytes wide, but the length is # of characters, not bytes.
                // 4-byte wide chars exist, but best I can tell Epic's (de)serializer doesn't support those.
                stringLength *= -2;

                // Read bytes representing string
                var bytes = br.ReadBytes(stringLength - 2);
                // Reading utf-16 two byte null terminators
                br.ReadBytes(2);

                return Encoding.Unicode.GetString(bytes);
            }

            // Handling ASCII
            if (stringLength > 0)
            {
                // Read bytes representing string
                var bytes = br.ReadBytes(stringLength - 1);
                // Read null delimiter
                br.ReadBytes(1);

                return Encoding.ASCII.GetString(bytes);
            }

            return "";
        }

        //TODO document
        public static void SkipReadingFstring(this BinaryReader br)
        {
            int stringLength = br.ReadInt32();
            br.BaseStream.Seek(stringLength, SeekOrigin.Current);
        }
    }
}