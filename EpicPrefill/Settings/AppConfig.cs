namespace EpicPrefill.Settings
{
    public static class AppConfig
    {
        static AppConfig()
        {
            // Create required folders
            Directory.CreateDirectory(ConfigDir);
            Directory.CreateDirectory(CacheDir);

            // Debugging folders
            Directory.CreateDirectory(DebugOutputDir);
            Directory.CreateDirectory(MetadataOutputDir);
            Directory.CreateDirectory(DownloadUrlPath);
        }

        /// <summary>
        /// Downloaded manifests, as well as other metadata, are saved into this directory to speedup future prefill runs.
        /// All data in here should be able to be deleted safely.
        /// </summary>
        public static readonly string CacheDir = TempDirUtils.GetTempDirBaseDirectories("EpicPrefill", CacheDirVersion);

        /// <summary>
        /// Increment when there is a breaking change made to the files in the cache directory
        /// </summary>
        private const string CacheDirVersion = "v1";

        /// <summary>
        /// Contains user configuration.  Should not be deleted, doing so will reset the app back to defaults.
        /// </summary>
        public static readonly string ConfigDir = Path.Combine(AppContext.BaseDirectory, "Config");

        //TODO comment
        public static int MaxConcurrentRequests => 30;

        //TODO comment
        private static bool _verboseLogs;
        public static bool VerboseLogs
        {
            get => _verboseLogs;
            set
            {
                _verboseLogs = value;
                AnsiConsoleExtensions.WriteVerboseLogs = value;
            }
        }

        #region Timeouts

        //TODO comment
        public static TimeSpan DefaultRequestTimeout => TimeSpan.FromSeconds(60);

        #endregion

        #region Serialization file paths

        public static readonly string AccountSettingsStorePath = Path.Combine(ConfigDir, "userAccount.json");

        public static readonly string UserSelectedAppsPath = Path.Combine(ConfigDir, "selectedAppsToPrefill.json");

        /// <summary>
        /// Keeps track of which apps + versions have been previously downloaded.  Is used to determine whether or not a game is up to date.
        /// </summary>
        public static readonly string SuccessfullyDownloadedAppsPath = Path.Combine(ConfigDir, "successfullyDownloadedApps.json");

        #endregion

        public static readonly string DefaultUserAgent = "EpicGamesLauncher/14.6.2-24350103+++Portal+Release-Live Windows/10.0.19044.1.256.64bit";

        #region Debugging

        public static readonly string DebugOutputDir = Path.Combine(CacheDir, "Debugging");
        public static readonly string MetadataOutputDir = Path.Combine(DebugOutputDir, "App Metadata");
        public static readonly string DownloadUrlPath = Path.Combine(DebugOutputDir, "Download URL");

        private static bool _debugLogs;
        public static bool DebugLogs
        {
            get => _debugLogs;
            set
            {
                _debugLogs = value;

                // Enable verbose logs as well
                VerboseLogs = true;
            }
        }

        /// <summary>
        /// Will skip over downloading chunks, but will still download manifests and build the chunk download list.  Useful for testing
        /// core logic of EpicPrefill without having to wait for downloads to finish.
        /// </summary>
        public static bool SkipDownloads { get; set; }

        /// <summary>
        /// Skips using locally cached manifests. Saves disk space, at the expense of slower subsequent runs.  Intended for debugging.
        /// </summary>
        public static bool NoLocalCache { get; set; }

        #endregion
    }
}