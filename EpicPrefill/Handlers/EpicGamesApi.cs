namespace EpicPrefill.Handlers
{
    /// <summary>
    /// This class is responsible for interacting with the Epic Games Online Services API.
    /// The methods here are intended to query the API and return the data as is with no transformations,
    /// however fields that are not needed by EpicPrefill will be omitted for the sake of simplicity.
    /// </summary>
    public sealed class EpicGamesApi
    {
        private readonly IAnsiConsole _ansiConsole;
        private readonly HttpClientFactory _httpClientFactory;

        private const string LauncherHost = "https://launcher-public-service-prod06.ol.epicgames.com";
        private const string CatalogHost = "https://catalog-public-service-prod06.ol.epicgames.com";

        private string MetadataCachePath => Path.Combine(AppConfig.CacheDir, "metadataCache.json");

        public EpicGamesApi(IAnsiConsole ansiConsole, HttpClientFactory httpClientFactory)
        {
            _ansiConsole = ansiConsole;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Gets a list of all owned apps for the currently logged in account.
        /// </summary>
        /// <returns></returns>
        internal async Task<List<Asset>> GetOwnedAppsAsync()
        {
            //TODO this should probably be inside a status spinner
            _ansiConsole.LogMarkupLine("Retrieving owned apps");
            var timer = Stopwatch.StartNew();

            // Build request
            var requestUri = new Uri($"{LauncherHost}/launcher/api/public/assets/Windows?label=Live");
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Send request
            using var httpClient = await _httpClientFactory.GetHttpClientAsync();
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            // Read and deserialize
            var responseContent = await response.Content.ReadAsStringAsync();
            var ownedApps = JsonSerializer.Deserialize(responseContent, SerializationContext.Default.ListAsset);

            // Dumping out the raw json if -debug is enabled
            if (AppConfig.DebugLogs)
            {
                await File.WriteAllTextAsync($@"{AppConfig.DebugOutputDir}\assetsResponse.json", responseContent);
            }

            // Removing anything related to unreal engine.  We're only interested in actual games
            var filteredApps = ownedApps.Where(e => e.Namespace != "ue")
                                                     // This namespace is related to unreal assets
                                                     .Where(e => e.Namespace != "89efe5924d3d467c839449ab6ab52e7f")
                                                     .ToList();

            _ansiConsole.LogMarkupLine($"Retrieved {Magenta(filteredApps.Count)} owned apps", timer);
            return filteredApps;
        }

        /// <summary>
        /// Gets additional metadata for the list of apps passed in.  The majority of this metadata isn't important to our use case, however we primarily need
        /// to get this metadata in order to know the apps titles.
        ///
        /// If the metadata has already been loaded before then a cached copy will be returned from disk.  If any of the apps are new and have not had their
        /// metadata loaded already then they will be requested and cached for future use.
        /// </summary>
        public async Task<Dictionary<string, AppMetadataResponse>> LoadAppMetadataAsync(List<Asset> apps)
        {
            var metadataDictionary = new Dictionary<string, AppMetadataResponse>();

            // Load cache from disk, if it exists
            if (File.Exists(MetadataCachePath))
            {
                var allText = await File.ReadAllTextAsync(MetadataCachePath);
                metadataDictionary = JsonSerializer.Deserialize(allText, SerializationContext.Default.DictionaryStringAppMetadataResponse);
            }

            // Determine which apps don't already have their metadata loaded
            List<Asset> appsMissingMetadata = apps.Where(e => !metadataDictionary.ContainsKey(e.AppId))
                                                      .OrderBy(e => e.AppId)
                                                      .ToList();

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
            await File.WriteAllTextAsync(MetadataCachePath, serialized);

            return metadataDictionary;
        }

        /// <summary>
        /// Gets additional metadata about a single app from Epic's API.  The only real thing that we are really interested in here is the app's title.
        /// </summary>
        private async Task<AppMetadataResponse> GetSingleAppMetadataAsync(Asset app)
        {
            // Building request
            var baseUrl = $"{CatalogHost}/catalog/api/shared/namespace/{app.Namespace}/bulk/items";
            var requestParams = new Dictionary<string, string>
            {
                { "id", app.CatalogItemId },
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

            var responseContent = await response.Content.ReadAsStringAsync();
            var appMetadata = JsonSerializer.Deserialize(responseContent, SerializationContext.Default.DictionaryStringAppMetadataResponse);

            // Dumping out the raw json if -debug is enabled
            if (AppConfig.DebugLogs)
            {
                await File.WriteAllTextAsync($@"{AppConfig.MetadataOutputDir}\{app.AppId}.json", responseContent);
            }

            return appMetadata.Values.First();
        }

        //TODO comment
        public async Task<ManifestUrl> GetManifestDownloadUrlAsync(AppInfo app)
        {
            var url = $"{LauncherHost}/launcher/api/public/assets/v2/platform/Windows/namespace/{app.Namespace}/" +
                            $"catalogItem/{app.CatalogItemId}/app/{app.AppId}/label/Live";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            using var httpClient = await _httpClientFactory.GetHttpClientAsync();
            using var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            ManifestResponse deserialized = JsonSerializer.Deserialize(responseContent, SerializationContext.Default.ManifestResponse);

            // Dumping out the raw json if -debug is enabled
            if (AppConfig.DebugLogs)
            {
                await File.WriteAllTextAsync($@"{AppConfig.DownloadUrlPath}\{app.AppId}.json", responseContent);
            }

            var allManifests = deserialized.elements.First().manifests.ToList();
            //TODO document why this is.  I don't remember exactly why it needs to be the one that has this query param.
            return allManifests.First(e => e.queryParams.Any(e2 => e2.Name == "f_token"));
        }
    }
}