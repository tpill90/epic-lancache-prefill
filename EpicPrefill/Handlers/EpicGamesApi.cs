namespace EpicPrefill.Handlers
{
    // TODO document
    public class EpicGamesApi
    {
        private readonly IAnsiConsole _ansiConsole;
        private readonly HttpClientFactory _httpClientFactory;

        private readonly string _launcherHost = "https://launcher-public-service-prod06.ol.epicgames.com";
        private readonly string _catalogHost = "https://catalog-public-service-prod06.ol.epicgames.com";
        private string _launcher_host = "https://launcher-public-service-prod06.ol.epicgames.com";

        private string _metadataCachePath => Path.Combine(AppConfig.CacheDir, "metadataCache.json");

        public EpicGamesApi(IAnsiConsole ansiConsole, HttpClientFactory httpClientFactory)
        {
            _ansiConsole = ansiConsole;
            _httpClientFactory = httpClientFactory;
        }

        //TODO comment
        public async Task<List<GameAsset>> GetOwnedAppsAsync()
        {
            //TODO this should probably be a status spinner
            _ansiConsole.LogMarkupLine("Retrieving owned apps");
            var timer = Stopwatch.StartNew();

            // Build request
            var requestUri = new Uri($"{_launcherHost}/launcher/api/public/assets/Windows?label=Live");
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Send request
            using var httpClient = await _httpClientFactory.GetHttpClientAsync();
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            // Read and deserialize
            using var responseContent = await response.Content.ReadAsStreamAsync();
            var ownedApps = JsonSerializer.Deserialize(responseContent, SerializationContext.Default.ListGameAsset);

            var appMetadata = await LoadAppMetadataAsync(ownedApps);
            foreach (var app in ownedApps)
            {
                app.Title = appMetadata[app.AppId].title;
            }

            _ansiConsole.LogMarkupLine($"Retrieved {Magenta(ownedApps.Count)} apps", timer);
            return ownedApps.OrderBy(e => e.Title).ToList();
        }

        //TODO comment
        private async Task<Dictionary<string, AppMetadataResponse>> LoadAppMetadataAsync(List<GameAsset> apps)
        {
            var metadataDictionary = new Dictionary<string, AppMetadataResponse>();

            // Load cache from disk, if it exists
            if (File.Exists(_metadataCachePath))
            {
                var allText = await File.ReadAllTextAsync(_metadataCachePath);
                metadataDictionary = JsonSerializer.Deserialize(allText, SerializationContext.Default.DictionaryStringAppMetadataResponse);
            }

            // Determine which apps don't already have their metadata loaded
            var appsMissingMetadata = apps.Where(e => !metadataDictionary.ContainsKey(e.AppId)).ToList();

            // If everything is cached, return
            if (!appsMissingMetadata.Any())
            {
                return metadataDictionary;
            }

            await _ansiConsole.CreateSpectreProgress().StartAsync(async context =>
            {
                var progressTask = context.AddTask("Loading app metadata...", maxValue: appsMissingMetadata.Count);

                foreach (var app in appsMissingMetadata)
                {
                    var metadata = await GetSingleAppMetadataAsync(app);
                    metadataDictionary.Add(app.AppId, metadata);
                    progressTask.Increment(1);
                }
            });

            _ansiConsole.LogMarkupLine($"Loaded new app metadata for {LightYellow(appsMissingMetadata.Count)} apps");

            // Serialize new metadata
            var serialized = JsonSerializer.Serialize(metadataDictionary, SerializationContext.Default.DictionaryStringAppMetadataResponse);
            await File.WriteAllTextAsync(_metadataCachePath, serialized);

            return metadataDictionary;
        }

        //TODO comment
        private async Task<AppMetadataResponse> GetSingleAppMetadataAsync(GameAsset appInfo)
        {
            // Building request
            var baseUrl = $"{_catalogHost}/catalog/api/shared/namespace/{appInfo.Namespace}/bulk/items";
            var requestParams = new Dictionary<string, string>
            {
                { "id", appInfo.CatalogItemId },
                { "includeDLCDetails", "true" },
                { "includeMainGameDetails", "True" },
                { "country", "US" },
                { "locale", "en" }
            };
            var urlWithParams = QueryHelpers.AddQueryString(baseUrl, requestParams);
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(urlWithParams));

            // Send request
            using var httpClient = await _httpClientFactory.GetHttpClientAsync();
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            var appMetadata = JsonSerializer.Deserialize(responseStream, SerializationContext.Default.DictionaryStringAppMetadataResponse);

            return appMetadata.Values.First();
        }

        //TODO comment
        public async Task<List<ManifestUrl>> GetAllDownloadUrlsAsync(GameAsset appInfo)
        {
            var url = $"{_launcher_host}/launcher/api/public/assets/v2/platform/Windows/namespace/{appInfo.Namespace}/" +
                      $"catalogItem/{appInfo.CatalogItemId}/app/{appInfo.AppId}/label/Live";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            using var httpClient = await _httpClientFactory.GetHttpClientAsync();
            using var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            ManifestResponse deserialized = JsonSerializer.Deserialize(responseStream, SerializationContext.Default.ManifestResponse);

            return deserialized.elements.First().manifests.ToList();
        }

        public ManifestUrl GetManifestDownloadUrl(List<ManifestUrl> allManifestUrls)
        {
            //TODO document why this is
            return allManifestUrls.First(e => e.queryParams.Any(e2 => e2.name == "f_token"));
        }
    }
}