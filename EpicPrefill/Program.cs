namespace EpicPrefill
{
    /* TODO
     * Research - Determine if Polly could be used in this project
     * Validate download sizes for various apps + apps with DLC
     * Auth links:
     * https://dev.epicgames.com/docs/web-api-ref/authentication?sessionInvalidated=true
     * https://github.com/claabs/epicgames-freegames-node
     * https://gist.github.com/iXyles/ec40cb40a2a186425ec6bfb9dcc2ddda
     */
    public static class Program
    {
        private const string Description = "Automatically fills a Lancache with games from Epic Launcher, so that subsequent downloads will be \n" +
            "  served from the Lancache, improving speeds and reducing load on your internet connection. \n" +
            "\n" +
            "  Start by selecting apps for prefill with the 'select-apps' command, then start the prefill using 'prefill'";

        public static async Task<int> Main()
        {
            //TODO dedupe exception handling at the top level.  Migrate to custom CLIFX binary just like SteamPrefill
            try
            {
                var cliArgs = ParseHiddenFlags();
                return await new CliApplicationBuilder()
                             .AddCommandsFromThisAssembly()
                             .SetTitle("EpicPrefill")
                             .SetExecutableNamePlatformAware("EpicPrefill")
                             .SetDescription(Description)
                             .SetVersion($"v{ThisAssembly.Info.InformationalVersion}")
                             .Build()
                             .RunAsync(cliArgs);
            }
            //TODO dedupe this throughout the codebase
            catch (TimeoutException e)
            {
                AnsiConsole.Console.MarkupLine("\n");
                //TODO implement
                //if (e.StackTrace.Contains(nameof(UserAccountStore.GetUsernameAsync)))
                //{
                //    AnsiConsole.Console.MarkupLine(Red("Timed out while waiting for username entry"));
                //}
                AnsiConsole.Console.LogException(e);
            }
            catch (TaskCanceledException e)
            {
                AnsiConsole.Console.LogException(e);
            }
            catch (Exception e)
            {
                AnsiConsole.Console.LogException(e);
            }

            return 0;
        }

        //TODO determine if this is useful
        /// <summary>
        /// Adds hidden flags that may be useful for debugging/development, but shouldn't be displayed to users in the help text
        /// </summary>
        private static List<string> ParseHiddenFlags()
        {
            // Have to skip the first argument, since its the path to the executable
            var args = Environment.GetCommandLineArgs().Skip(1).ToList();

            // Will skip over downloading logic.  Will only download manifests
            if (args.Any(e => e.Contains("--no-download")))
            {
                AnsiConsole.Console.LogMarkupLine($"Using {LightYellow("--no-download")} flag.  Will skip downloading chunks...");
                AppConfig.SkipDownloads = true;
                args.Remove("--no-download");
            }

            // Skips using locally cached manifests. Saves disk space, at the expense of slower subsequent runs.
            // Useful for debugging since the manifests will always be re-downloaded.
            if (args.Any(e => e.Contains("--nocache")) || args.Any(e => e.Contains("--no-cache")))
            {
                AnsiConsole.Console.LogMarkupLine($"Using {LightYellow("--nocache")} flag.  Will always re-download manifests...");
                AppConfig.NoLocalCache = true;
                args.Remove("--nocache");
                args.Remove("--no-cache");
            }

            // Adding some formatting to logging to make it more readable + clear that these flags are enabled
            if (AppConfig.SkipDownloads || AppConfig.NoLocalCache)
            {
                AnsiConsole.Console.WriteLine();
                AnsiConsole.Console.Write(new Rule());
            }

            return args;
        }
    }
}