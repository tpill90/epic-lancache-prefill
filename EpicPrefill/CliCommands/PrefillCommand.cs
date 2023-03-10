// ReSharper disable MemberCanBePrivate.Global - Properties used as parameters can't be private with CliFx, otherwise they won't work.
namespace EpicPrefill.CliCommands
{
    //TODO will need to check for valid auth after every prefill attempt
    [UsedImplicitly]
    [Command("prefill", Description = "Downloads the latest version of one or more specified app(s)." +
                                           "  Automatically includes apps selected using the 'select-apps' command")]
    public class PrefillCommand : ICommand
    {

#if DEBUG // Development/debugging only

        //TODO implement
        [CommandOption("app", Description = "Debugging only.")]
        public List<string> AppIds { get; init; }

        [CommandOption("no-download", Description = "Debugging only.", Converter = typeof(NullableBoolConverter))]
        public bool? NoDownload
        {
            get => AppConfig.SkipDownloads;
            init => AppConfig.SkipDownloads = value ?? default(bool);
        }
#endif

        #region Cli Args

        //TODO implement
        [CommandOption("all", Description = "Prefills all currently owned games", Converter = typeof(NullableBoolConverter))]
        public bool? DownloadAllOwnedGames { get; init; }

        //TODO implement
        [CommandOption("force", 'f',
            Description = "Forces the prefill to always run, overrides the default behavior of only prefilling if a newer version is available.",
            Converter = typeof(NullableBoolConverter))]
        public bool? Force { get; init; }

        //TODO implement
        [CommandOption("nocache",
            Description = "Skips using locally cached files. Saves disk space, at the expense of slower subsequent runs.",
            Converter = typeof(NullableBoolConverter))]
        public bool? NoLocalCache { get; init; }

        [CommandOption("verbose", Description = "Produces more detailed log output. Will output logs for games are already up to date.", Converter = typeof(NullableBoolConverter))]
        public bool? Verbose
        {
            get => AppConfig.VerboseLogs;
            init => AppConfig.VerboseLogs = value ?? default(bool);
        }

        [CommandOption("unit",
            Description = "Specifies which unit to use to display download speed.  Can be either bits/bytes.",
            Converter = typeof(TransferSpeedUnitConverter))]
        public TransferSpeedUnit TransferSpeedUnit { get; init; } = TransferSpeedUnit.Bits;

        [CommandOption("no-ansi",
            Description = "Application output will be in plain text.  " +
                          "Should only be used if terminal does not support Ansi Escape sequences, or when redirecting output to a file.",
            Converter = typeof(NullableBoolConverter))]
        public bool? NoAnsiEscapeSequences { get; init; }

        #endregion

        private IAnsiConsole _ansiConsole;

        //TODO readd update check
        //TODO need to validate that the user has selected any apps, or they have specified --all
        //TODO print download size, time taken to complete download, and implement the prefill result logic
        public async ValueTask ExecuteAsync(IConsole console)
        {
            _ansiConsole = console.CreateAnsiConsole();
            // Property must be set to false in order to disable ansi escape sequences
            _ansiConsole.Profile.Capabilities.Ansi = !NoAnsiEscapeSequences ?? true;

            var downloadArgs = new DownloadArguments
            {
                Force = Force ?? default(bool),
                NoCache = NoLocalCache ?? default(bool),
                TransferSpeedUnit = TransferSpeedUnit
            };

            var epicGamesManager = new EpicGamesManager(_ansiConsole, downloadArgs);
            await epicGamesManager.InitializeAsync();

            //TODO clean this up to be how SteamPrefill does it
            var manualIds = new List<string>();
#if DEBUG
            manualIds = AppIds;
#endif
            await epicGamesManager.DownloadMultipleAppsAsync(DownloadAllOwnedGames ?? default(bool), manualIds);
        }
    }
}