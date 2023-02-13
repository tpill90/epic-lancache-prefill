namespace Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class BigEndianConversion
    {
        private readonly int iterations = 1_500_000;
        private readonly MemoryStream _stream;
        private readonly BinaryReader br;

        public BigEndianConversion()
        {
            var inputBytes = new byte[iterations * 16];

            var random = new Random();
            random.NextBytes(inputBytes);

            _stream = new MemoryStream(inputBytes);
            br = new BinaryReader(_stream);
        }

        [Benchmark(Baseline = true)]
        public string TimOptimized()
        {
            // Reset stream
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            var resultString = "";

            byte[] sharedBuffer = new byte[16];
            for (var i = 0; i < iterations; i++)
            {
                br.Read(sharedBuffer);

                sharedBuffer.AsSpan(0, 4).Reverse();
                sharedBuffer.AsSpan(4, 4).Reverse();
                sharedBuffer.AsSpan(8, 4).Reverse();
                sharedBuffer.AsSpan(12, 4).Reverse();

                resultString = sharedBuffer.ToHexStringUpper();
            }
            return resultString;
        }
    }
}