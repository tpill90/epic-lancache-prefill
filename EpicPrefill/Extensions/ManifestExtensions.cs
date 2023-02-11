namespace EpicPrefill.Extensions
{
    public static class ManifestExtensions
    {
        //TODO should this live with the json manifest itself?
        //TODO refactor + document + rename
        public static ulong BlobToNum(this string input)
        {
            // Converts the input string representing a big endian ulong, into the correct decimal value.
            // Input string is an array of bytes, converted to its decimal value, and concatenated with each other to a string.
            // For example 056217002 should read as "056 217 002"
            var byteArray = new byte[16];

            for (int i = 0; i < input.Length / 3; i++)
            {
                ReadOnlySpan<char> span = input.AsSpan(i * 3, 3);
                byteArray[i] = byte.Parse(span);
            }

            return BitConverter.ToUInt64(byteArray);
        }
    }
}