namespace Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class BlobToNumBenchmark
    {
        private readonly int iterations = 500_000;
        
        [Benchmark(Baseline = true)]
        public ulong Original()
        {
            var inputString = "193157029079132122245178";
            ulong result = 0;
            for (int i = 0; i < iterations; i++)
            {
                result = BlobToNumOriginal(inputString);
            }
            return result;
        }

        [Benchmark]
        public ulong Refactored()
        {
            var inputString = "193157029079132122245178";
            ulong result = 0;
            for (int i = 0; i < iterations; i++)
            {
                result = inputString.BlobToNum();
            }
            return result;
        }

        public ulong BlobToNumOriginal(string input)
        {
            // Converts the input string representing a big endian ulong, into the correct decimal value.
            // Input string is an array of bytes, converted to its decimal value, and concatenated with each other to a string.
            // For example 056217002 should read as "056 217 002"
            var chunks = input.Chunk(3)
                              .Select(e => new string(e))
                              .ToList();

            ulong result = 0;
            for (int i = 0; i < chunks.Count; i++)
            {
                // Take the 3 character string, and parse it into its decimal numerical value.
                ulong decimalValue = ulong.Parse(chunks[i]);

                // Need to reverse endianness, so we will bit shift
                ulong shifted = decimalValue << (8 * i);

                result += shifted;
            }

            return result;
        }
    }
}