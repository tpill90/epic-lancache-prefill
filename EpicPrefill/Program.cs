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
        public static async Task<int> Main()
        {
            try
            {
                var cliArgs = ParseHiddenFlags();
                var description = "Automatically fills a Lancache with games from Epic Launcher, so that subsequent downloads will be \n" +
                                  "  served from the Lancache, improving speeds and reducing load on your internet connection. \n" +
                                  "\n" +
                                  "  Start by selecting apps for prefill with the 'select-apps' command, then start the prefill using 'prefill'";

                return await new CliApplicationBuilder()
                             .AddCommandsFromThisAssembly()
                             .SetTitle("EpicPrefill")
                             .SetExecutableName($"EpicPrefill{(OperatingSystem.IsWindows() ? ".exe" : "")}")
                             .SetDescription(description)
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
                if (e.StackTrace.Contains(nameof(SpectreConsoleExtensions.ReadPasswordAsync)))
                {
                    AnsiConsole.Console.MarkupLine(Red("Timed out while waiting for password entry"));
                }
                AnsiConsole.Console.WriteException(e, ExceptionFormats.ShortenPaths);
            }
            catch (TaskCanceledException e)
            {
                AnsiConsole.Console.WriteException(e, ExceptionFormats.ShortenPaths);
            }
            catch (Exception e)
            {
                AnsiConsole.Console.WriteException(e, ExceptionFormats.ShortenPaths);
            }

            return 0;
        }

        //TODO determine if this is useful
        /// <summary>
        /// Adds hidden flags that may be useful for debugging/development, but shouldn't be displayed to users in the help text
        /// </summary>
        public static List<string> ParseHiddenFlags()
        {
            // Have to skip the first argument, since its the path to the executable
            var args = Environment.GetCommandLineArgs().Skip(1).ToList();

            if (args.Any(e => e.Contains("--debug")))
            {
                args.Remove("--debug");
            }

            return args;
        }

        //TODO move to lancache prefill common
        public static class OperatingSystem
        {
            public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}