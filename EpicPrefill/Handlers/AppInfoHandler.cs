namespace EpicPrefill.Handlers
{
    //TODO rename
    public sealed class AppInfoHandler
    {
        private readonly IAnsiConsole _ansiConsole;

        //TODO document
        private readonly Dictionary<string, HashSet<string>> _previouslyDownloadedApps = new Dictionary<string, HashSet<string>>();

        public AppInfoHandler(IAnsiConsole ansiConsole)
        {
            _ansiConsole = ansiConsole;
            if (File.Exists(AppConfig.SuccessfullyDownloadedAppsPath))
            {
                var fileContents = File.ReadAllText(AppConfig.SuccessfullyDownloadedAppsPath);
                _previouslyDownloadedApps = JsonSerializer.Deserialize(fileContents, SerializationContext.Default.DictionaryStringHashSetString);
            }
        }

        public void MarkDownloadAsSuccessful(GameAsset appInfo)
        {
            // Initialize the entry for the specified app
            if (!_previouslyDownloadedApps.ContainsKey(appInfo.AppId))
            {
                _previouslyDownloadedApps.Add(appInfo.AppId, new HashSet<string>());
            }

            var downloadedAppVersions = _previouslyDownloadedApps[appInfo.AppId];
            if (!downloadedAppVersions.Contains(appInfo.BuildVersion))
            {
                downloadedAppVersions.Add(appInfo.BuildVersion);
            }

            var serialized = JsonSerializer.Serialize(_previouslyDownloadedApps, SerializationContext.Default.DictionaryStringHashSetString);
            File.WriteAllText(AppConfig.SuccessfullyDownloadedAppsPath, serialized);
        }

        /// <summary>
        /// An app will be considered up to date if it's current build version has been previously downloaded.
        /// </summary>
        public bool AppIsUpToDate(GameAsset appInfo)
        {
            //TODO validate that the build version increments when a new version is released
            // If this version has never been downloaded at all, will always be false
            if (!_previouslyDownloadedApps.ContainsKey(appInfo.AppId))
            {
                return false;
            }

            return _previouslyDownloadedApps[appInfo.AppId].Contains(appInfo.BuildVersion);
        }
    }
}