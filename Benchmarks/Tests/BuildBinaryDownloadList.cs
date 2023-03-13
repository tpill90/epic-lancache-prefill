//using EpicPrefill.Models.ApiResponses;
//using EpicPrefill.Models.Manifests;

//namespace Benchmarks.Tests
//{
//    [MemoryDiagnoser]
//    public class BuildBinaryDownloadList
//    {
//        private IAnsiConsole _ansiConsole = new TestConsole();
//        private BinaryManifest _manifest;
//        private EpicGamesManager _epicGamesManager;

//        public BuildBinaryDownloadList()
//        {
//            //var manifestFilePath = @"C:\\Users\\Tim\\AppData\\Local\\EpicPrefill\\Cache\\v1\\Catnip-1.0.23_CL_2860060_Borderlands_3";
//            //var rawManifestBytes = File.ReadAllBytes(manifestFilePath);

//            //var baseUri = "https://fastly-download.epicgames.com/Builds/Org/o-37m6jbj5wcvrcvm4wusv7nazdfvbjk/Catnip/default/zLv4U336onfhxULA6USZZ57Ag9h5aA.manifest";
//            //_manifest = BinaryManifest.Parse(rawManifestBytes, new ManifestUrl() { uri = baseUri });
//            //_epicGamesManager = new EpicGamesManager(new TestConsole(), new DownloadArguments());
//        }

//        //[Benchmark]
//        //public List<QueuedRequest> BuildBinaryDownload_Current()
//        //{
//        //    return _epicGamesManager.BuildDownloadQueue(_manifest);
//        //}
//    }
//}
