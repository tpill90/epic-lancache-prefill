namespace EpicPrefill.Handlers
{
    public sealed class ManifestHandler
    {
        private readonly IAnsiConsole _ansiConsole;
        private readonly HttpClientFactory _httpClientFactory;
        private readonly DownloadArguments _downloadArguments;

        public ManifestHandler(IAnsiConsole ansiConsole, HttpClientFactory httpClientFactory, DownloadArguments downloadArguments)
        {
            _ansiConsole = ansiConsole;
            _httpClientFactory = httpClientFactory;
            _downloadArguments = downloadArguments;
        }

        /// <summary>
        /// Downloads the latest manifest for the specified app.
        /// Only downloads the raw response, doesn't do any further processing.
        /// To parse the manifest into a useable form, "/>
        /// </summary>
        /// <returns>The raw bytes for the downloaded manifest</returns>
        public async Task<byte[]> DownloadManifestAsync(GameAsset appInfo, ManifestUrl manifestDownloadUrl)
        {
            // Load from disk if manifest already exists
            var cachedFileName = Path.Combine(AppConfig.CacheDir, $"{appInfo.AppId}-{appInfo.BuildVersion}");
            if (ManifestIsCached(cachedFileName))
            {
                return await File.ReadAllBytesAsync(cachedFileName);
            }

            byte[] responseAsBytes = null;
            await _ansiConsole.StatusSpinner().StartAsync("Downloading manifest", async ctx =>
            {
                var timer = Stopwatch.StartNew();
                using var request = new HttpRequestMessage(HttpMethod.Get, manifestDownloadUrl.ManifestDownloadUrlWithParams);

                using var httpClient = await _httpClientFactory.GetHttpClientAsync();
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                responseAsBytes = await response.Content.ReadAsByteArrayAsync();
                // Cache to disk
                await File.WriteAllBytesAsync(cachedFileName, responseAsBytes);

                _ansiConsole.LogMarkupLine("Downloaded manifest", timer);
            });
            return responseAsBytes;
        }

        private bool ManifestIsCached(string manifestFileName)
        {
            return !AppConfig.NoLocalCache && File.Exists(manifestFileName);
        }

        //TODO document
        public List<QueuedRequest> ParseManifest(byte[] rawManifestBytes, ManifestUrl manifestDownloadUrl)
        {
            List<QueuedRequest> chunkDownloadQueue = null;
            _ansiConsole.StatusSpinner().Start("Parsing download manifest", ctx =>
            {
                var timer = Stopwatch.StartNew();

                // For whatever reason, EGS manifests are in two different formats : JSON + Binary.
                // We will determine which format is used, and parse it appropriately
                if (rawManifestBytes[0] == '{')
                {
                    // Deserialize JSON Manifest
                    JsonManifest manifest = JsonSerializer.Deserialize(rawManifestBytes, SerializationContext.Default.JsonManifest);
                    chunkDownloadQueue = BuildDownloadQueue(manifest, manifestDownloadUrl);
                }
                else
                {
                    // Otherwise Manifest is in a binary format
                    BinaryManifest manifest = BinaryManifest.Parse(rawManifestBytes, manifestDownloadUrl);
                    chunkDownloadQueue = BuildDownloadQueue(manifest);
                }

                _ansiConsole.LogMarkupVerbose("Parsed manifest + built download queue", timer);
            });
            return chunkDownloadQueue;
        }

        //TODO should the ManifestUrl.BasePath be refactored out?
        private List<QueuedRequest> BuildDownloadQueue(JsonManifest jsonManifest, ManifestUrl manifestUrl)
        {
            var guids = jsonManifest.ChunkHashList.Keys.ToList();

            var downloadList = new List<QueuedRequest>();
            foreach (var guid in guids)
            {
                string hashHexString = jsonManifest.ChunkHashList[guid].BlobToNum().ToString("X16");
                string groupNum = jsonManifest.DataGroupList[guid].BlobToNum().ToString("D2");

                var downloadChunk = new QueuedRequest
                {
                    //TODO refactor this, hard to read
                    DownloadUrl = Path.Join(manifestUrl.ChunkBaseUrl, jsonManifest.GetChunkDir(), groupNum, $"{hashHexString}_{guid}.chunk"),
                    DownloadSizeBytes = jsonManifest.ChunkFilesizeList[guid].BlobToNum()
                };
                downloadList.Add(downloadChunk);
            }

            return downloadList;
        }

        private List<QueuedRequest> BuildDownloadQueue(BinaryManifest binaryManifest)
        {
            var downloadQueue = binaryManifest.ChunkDataLookup.Values
                                             .DistinctBy(e => e.Guid)
                                             .Select(chunk => new QueuedRequest
                                             {
                                                 DownloadSizeBytes = chunk.CompressedFileSize,
                                                 DownloadUrl = Path.Combine(binaryManifest.Url.ChunkBaseUrl, chunk.Uri)
                                             })
                                             .ToList();

            return downloadQueue;
        }
    }
}