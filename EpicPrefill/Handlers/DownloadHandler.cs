namespace EpicPrefill.Handlers
{
    public sealed class DownloadHandler : IDisposable
    {
        private readonly IAnsiConsole _ansiConsole;
        private readonly HttpClient _client;

        /// <summary>
        /// The URL/IP Address where the Lancache has been detected.
        /// </summary>
        private string _lancacheAddress;

        public DownloadHandler(IAnsiConsole ansiConsole)
        {
            _ansiConsole = ansiConsole;

            _client = new HttpClient
            {
                Timeout = AppConfig.DefaultRequestTimeout
            };
            _client.DefaultRequestHeaders.Add("User-Agent", AppConfig.DefaultUserAgent);
        }

        //TODO document allManifestUrls
        /// <summary>
        /// Attempts to download all queued requests.  If all downloads are successful, will return true.
        /// In the case of any failed downloads, the failed downloads will be retried up to 3 times.  If the downloads fail 3 times, then
        /// false will be returned
        /// </summary>
        /// <returns>True if all downloads succeeded.  False if downloads failed 3 times.</returns>
        public async Task<bool> DownloadQueuedChunksAsync(List<QueuedRequest> queuedRequests, List<ManifestUrl> allManifestUrls)
        {
#if DEBUG
            if (AppConfig.SkipDownloads)
            {
                return true;
            }
#endif
            if (_lancacheAddress == null)
            {
                var cdnUrl = allManifestUrls.First().ManifestDownloadUri.Host;
                _lancacheAddress = await LancacheIpResolver.ResolveLancacheIpAsync(_ansiConsole, cdnUrl);
            }

            int retryCount = 0;
            var failedRequests = new ConcurrentBag<QueuedRequest>();
            await _ansiConsole.CreateSpectreProgress(TransferSpeedUnit.Bits).StartAsync(async ctx =>
            {
                //TODO should probably implement cycling through available CDNs when one fails
                // Run the initial download
                failedRequests = await AttemptDownloadAsync(ctx, "Downloading..", queuedRequests, new Uri(allManifestUrls.First().ManifestDownloadUrl));

                // Handle any failed requests
                while (failedRequests.Any() && retryCount < 2)
                {
                    retryCount++;
                    await Task.Delay(2000 * retryCount);
                    var upstreamCdn = new Uri(allManifestUrls.First().ManifestDownloadUrl);
                    failedRequests = await AttemptDownloadAsync(ctx, $"Retrying  {retryCount}..", failedRequests.ToList(), upstreamCdn, forceRecache: true);
                }
            });

            // Handling final failed requests
            if (!failedRequests.Any())
            {
                return true;
            }

            _ansiConsole.LogMarkupError($"Download failed with {LightYellow(failedRequests.Count)} failed requests");
            _ansiConsole.WriteLine();
            return false;
        }

        //TODO I don't like the number of parameters here, should maybe rethink the way this is written.
        /// <summary>
        /// Attempts to download the specified requests.  Returns a list of any requests that have failed for any reason.
        /// </summary>
        /// <param name="forceRecache">When specified, will cause the cache to delete the existing cached data for a request, and redownload it again.</param>
        /// <returns>A list of failed requests</returns>
        [SuppressMessage("Reliability", "CA2016:Forward the 'CancellationToken' parameter to methods", Justification = "Don't have a need to cancel")]
        private async Task<ConcurrentBag<QueuedRequest>> AttemptDownloadAsync(ProgressContext ctx, string taskTitle, List<QueuedRequest> requestsToDownload,
                                                                                Uri upstreamCdn, bool forceRecache = false)
        {
            double requestTotalSize = requestsToDownload.Sum(e => (long)e.DownloadSizeBytes);
            var progressTask = ctx.AddTask(taskTitle, new ProgressTaskSettings { MaxValue = requestTotalSize });

            var failedRequests = new ConcurrentBag<QueuedRequest>();

            await Parallel.ForEachAsync(requestsToDownload, new ParallelOptions { MaxDegreeOfParallelism = AppConfig.MaxConcurrentRequests }, async (chunk, _) =>
            {
                var buffer = new byte[4096];
                try
                {
                    var url = Path.Join($"http://{_lancacheAddress}", chunk.DownloadUrl);
                    if (forceRecache)
                    {
                        url += "?nocache=1";
                    }

                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                    requestMessage.Headers.Host = upstreamCdn.Host;

                    var response = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                    using Stream responseStream = await response.Content.ReadAsStreamAsync();
                    response.EnsureSuccessStatusCode();

                    // Don't save the data anywhere, so we don't have to waste time writing it to disk.
                    while (await responseStream.ReadAsync(buffer, 0, buffer.Length, _) != 0)
                    {
                    }
                }
                catch (Exception e)
                {
                    failedRequests.Add(chunk);
                    FileLogger.LogExceptionNoStackTrace($"Request {chunk.DownloadUrl}", e);
                }
                progressTask.Increment(chunk.DownloadSizeBytes);
            });

            // Making sure the progress bar is always set to its max value, in-case some unexpected error leaves the progress bar showing as unfinished
            progressTask.Increment(progressTask.MaxValue);
            return failedRequests;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}