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

        [Benchmark]
        public string Reverse_OneByte_PerGuid()
        {
            // Reset stream
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            var resultString = "";

            byte[] sharedBuffer = new byte[16];
            for (var i = 0; i < iterations; i++)
            {
                br.Read(sharedBuffer);
                byte tmp = sharedBuffer[0];
                sharedBuffer[0] = sharedBuffer[3];
                sharedBuffer[3] = tmp;
                tmp = sharedBuffer[1];
                sharedBuffer[1] = sharedBuffer[2];
                sharedBuffer[2] = tmp;

                tmp = sharedBuffer[4];
                sharedBuffer[4] = sharedBuffer[7];
                sharedBuffer[7] = tmp;
                tmp = sharedBuffer[5];
                sharedBuffer[5] = sharedBuffer[6];
                sharedBuffer[6] = tmp;

                tmp = sharedBuffer[8];
                sharedBuffer[8] = sharedBuffer[11];
                sharedBuffer[11] = tmp;
                tmp = sharedBuffer[9];
                sharedBuffer[9] = sharedBuffer[10];
                sharedBuffer[10] = tmp;

                tmp = sharedBuffer[12];
                sharedBuffer[12] = sharedBuffer[15];
                sharedBuffer[15] = tmp;
                tmp = sharedBuffer[13];
                sharedBuffer[13] = sharedBuffer[14];
                sharedBuffer[14] = tmp;

                resultString = sharedBuffer.ToHexStringUpper();
            }
            return resultString;
        }

        [Benchmark]
        public string Reverse_OneByte_PerGuid_WithLoop()
        {
            // Reset stream
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            var resultString = "";

            byte[] sharedBuffer = new byte[16];
            for (var i = 0; i < iterations; i++)
            {
                br.Read(sharedBuffer);
                byte tmp;
                for (int j = 0; j < 4; j++)
                {
                    tmp = sharedBuffer[j];
                    sharedBuffer[j] = sharedBuffer[j + 3];
                    sharedBuffer[j + 3] = tmp;
                    tmp = sharedBuffer[j + 1];
                    sharedBuffer[j + 1] = sharedBuffer[j + 2];
                    sharedBuffer[j + 2] = tmp;
                }

                resultString = sharedBuffer.ToHexStringUpper();
            }
            return resultString;
        }

        [Benchmark]
        public string Reverse_OneByte_AcrossAllGuids()
        {
            // Reset stream
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            var resultString = "";

            byte[] sharedBuffer = new byte[16];
            byte tmp;
            for (var i = 0; i < iterations; i++)
            {
                br.Read(sharedBuffer);
                tmp = sharedBuffer[0];
                sharedBuffer[0] = sharedBuffer[3];
                sharedBuffer[3] = tmp;
                tmp = sharedBuffer[1];
                sharedBuffer[1] = sharedBuffer[2];
                sharedBuffer[2] = tmp;

                tmp = sharedBuffer[4];
                sharedBuffer[4] = sharedBuffer[7];
                sharedBuffer[7] = tmp;
                tmp = sharedBuffer[5];
                sharedBuffer[5] = sharedBuffer[6];
                sharedBuffer[6] = tmp;

                tmp = sharedBuffer[8];
                sharedBuffer[8] = sharedBuffer[11];
                sharedBuffer[11] = tmp;
                tmp = sharedBuffer[9];
                sharedBuffer[9] = sharedBuffer[10];
                sharedBuffer[10] = tmp;

                tmp = sharedBuffer[12];
                sharedBuffer[12] = sharedBuffer[15];
                sharedBuffer[15] = tmp;
                tmp = sharedBuffer[13];
                sharedBuffer[13] = sharedBuffer[14];
                sharedBuffer[14] = tmp;

                resultString = sharedBuffer.ToHexStringUpper();
            }
            return resultString;
        }

        [Benchmark]
        public string Reverse_TwiddlyBits()
        {
            // Reset stream
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            var resultString = "";

            byte[] sharedBuffer = new byte[16];
            for (var i = 0; i < iterations; i++)
            {
                br.Read(sharedBuffer);

                sharedBuffer[0] = (byte)(sharedBuffer[0] ^ sharedBuffer[3] ^ (sharedBuffer[3] = sharedBuffer[0]));
                sharedBuffer[1] = (byte)(sharedBuffer[1] ^ sharedBuffer[2] ^ (sharedBuffer[2] = sharedBuffer[1]));

                sharedBuffer[4] = (byte)(sharedBuffer[4] ^ sharedBuffer[7] ^ (sharedBuffer[7] = sharedBuffer[4]));
                sharedBuffer[5] = (byte)(sharedBuffer[5] ^ sharedBuffer[6] ^ (sharedBuffer[6] = sharedBuffer[5]));

                sharedBuffer[8] = (byte)(sharedBuffer[8] ^ sharedBuffer[11] ^ (sharedBuffer[11] = sharedBuffer[8]));
                sharedBuffer[9] = (byte)(sharedBuffer[9] ^ sharedBuffer[10] ^ (sharedBuffer[10] = sharedBuffer[9]));

                sharedBuffer[12] = (byte)(sharedBuffer[12] ^ sharedBuffer[15] ^ (sharedBuffer[15] = sharedBuffer[12]));
                sharedBuffer[13] = (byte)(sharedBuffer[13] ^ sharedBuffer[14] ^ (sharedBuffer[14] = sharedBuffer[13]));

                resultString = sharedBuffer.ToHexStringUpper();
            }
            return resultString;
        }
    }
}