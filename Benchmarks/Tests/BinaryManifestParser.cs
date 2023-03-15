using EpicPrefill.Models.ApiResponses;
using EpicPrefill.Models.Manifests;

namespace Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class BinaryManifestParser
    {
        private byte[] _rawManifestBytes;
        private ManifestUrl manifestDownloadUrl = new ManifestUrl();
        private IAnsiConsole _ansiConsole = new TestConsole();


        public BinaryManifestParser()
        {
            var manifestFilePath = @"C:\\Users\\Tim\\AppData\\Local\\EpicPrefill\\Cache\\v1\\Catnip-1.0.23_CL_2860060_Borderlands_3";
            _rawManifestBytes = File.ReadAllBytes(manifestFilePath);
        }

        //[Benchmark(Baseline = true)]
        //public BaselineBinaryManifest Original()
        //{
        //    return BaselineBinaryManifest.Parse(_ansiConsole, _rawManifestBytes, manifestDownloadUrl);
        //}

        //[Benchmark]
        //public BinaryManifest New()
        //{
        //    return BinaryManifest.Parse(_rawManifestBytes, manifestDownloadUrl);
        //}
    }
}
