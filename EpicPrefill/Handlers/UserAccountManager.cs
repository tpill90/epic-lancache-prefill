namespace EpicPrefill.Handlers
{
    //TODO document
    /// <summary>
    /// https://dev.epicgames.com/docs/web-api-ref/authentication
    /// </summary>
    //TODO fix this warning
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Fix this.")]
    public sealed class UserAccountManager
    {
        private readonly IAnsiConsole _ansiConsole;
        private readonly HttpClient _client;

        //TODO I'm not sure where this link comes from.  Can I possibly setup my own?
        private const string LoginUrl = "https://legendary.gl/epiclogin";
        private const string OauthHost = "account-public-service-prod03.ol.epicgames.com";

        private const string BasicUsername = "34a02cf8f4414e29b15921876da36f9a";
        private const string BasicPassword = "daafbccc737745039dffe53d94fc76cf";

        private const int MaxRetries = 3;

        //TODO this should probably be private
        public OauthToken OauthToken { get; set; }

        private UserAccountManager(IAnsiConsole ansiConsole)
        {
            _ansiConsole = ansiConsole;
            _client = new HttpClient
            {
                Timeout = AppConfig.DefaultRequestTimeout
            };
            _client.DefaultRequestHeaders.Add("User-Agent", AppConfig.DefaultUserAgent);
        }

        public async Task LoginAsync()
        {
            if (!OauthTokenIsExpired())
            {
                _ansiConsole.LogMarkupLine("Reusing existing auth session...");
                return;
            }

            int retryCount = 0;
            while (OauthTokenIsExpired() && retryCount < MaxRetries)
            {
                try
                {
                    var requestParams = BuildRequestParams();

                    var authUri = new Uri($"https://{OauthHost}/account/api/oauth/token");
                    using var request = new HttpRequestMessage(HttpMethod.Post, authUri);
                    request.Headers.Authorization = BasicAuthentication.ToAuthenticationHeader(BasicUsername, BasicPassword);
                    request.Content = new FormUrlEncodedContent(requestParams);

                    using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    OauthToken = JsonSerializer.Deserialize(responseStream, SerializationContext.Default.OauthToken);

                    Save();
                }
                catch (Exception e)
                {
                    // If the login failed due to a bad request then we'll clear out the existing token and try again
                    if (e is HttpRequestException)
                    {
                        OauthToken = null;
                    }
                }

                retryCount++;
            }

            if (retryCount == 3)
            {
                throw new EpicLoginException("Unable to login to Epic!  Try again in a few moments...");
            }
        }

        private Dictionary<string, string> BuildRequestParams()
        {
            // Handles the user logging in for the first time, as well as when the refresh token has expired, or when an unknown failure has occurred
            if (OauthToken == null || RefreshTokenIsExpired())
            {
                if (RefreshTokenIsExpired())
                {
                    _ansiConsole.LogMarkupLine(LightYellow("Refresh token has expired!  EpicPrefill will need to login again..."));
                }

                _ansiConsole.LogMarkupLine("Please login into Epic via your browser");
                _ansiConsole.LogMarkupLine($"If the web page did not open automatically, please manually open the following URL: {Cyan(LoginUrl)}");
                OpenUrl(LoginUrl);

                //TODO handle users pasting in the entire JSON response.  Or figure out a way to not require doing this at all.
                //TODO might be able to do username + password without browser : https://gist.github.com/iXyles/ec40cb40a2a186425ec6bfb9dcc2ddda
                var authCode = _ansiConsole.Prompt(new TextPrompt<string>($"Please enter the {LightYellow("authorizationCode")} from the JSON response:"));

                return new Dictionary<string, string>
                {
                    { "token_type", "eg1" },
                    { "grant_type", "authorization_code" },
                    { "code", authCode }
                };
            }

            // Handles a user being logged in, but the saved token has expired
            _ansiConsole.LogMarkupLine("Auth token expired.  Requesting refresh auth token...");
            return new Dictionary<string, string>
            {
                { "token_type", "eg1" },
                { "grant_type", "refresh_token" },
                { "refresh_token", OauthToken.RefreshToken }
            };
        }

        //TODO this should probably not be referenced externally
        public bool OauthTokenIsExpired()
        {
            if (OauthToken == null)
            {
                return true;
            }

            // Tokens are valid for 8 hours, but we're adding a buffer of 10 minutes to make sure that the token doesn't expire while we're using it.
            return DateTimeOffset.UtcNow.DateTime > OauthToken.ExpiresAt.AddMinutes(-10);
        }

        private bool RefreshTokenIsExpired()
        {
            if (OauthToken == null)
            {
                return true;
            }

            // Tokens are valid for 8 hours, but we're adding a buffer of 10 minutes to make sure that the token doesn't expire while we're using it.
            return DateTimeOffset.UtcNow.DateTime > OauthToken.RefreshTokenExpiresAt;
        }

        //TODO document
        public static UserAccountManager LoadFromFile(IAnsiConsole ansiConsole)
        {
            if (!File.Exists(AppConfig.AccountSettingsStorePath))
            {
                return new UserAccountManager(ansiConsole);
            }

            using var fileStream = File.Open(AppConfig.AccountSettingsStorePath, FileMode.Open, FileAccess.Read);

            var accountManager = new UserAccountManager(ansiConsole);
            accountManager.OauthToken = JsonSerializer.Deserialize(fileStream, SerializationContext.Default.OauthToken);
            return accountManager;
        }

        private void Save()
        {
            using var fileStream = File.Open(AppConfig.AccountSettingsStorePath, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(fileStream, OauthToken, SerializationContext.Default.OauthToken);
        }

        private void OpenUrl(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Detects to see if the user is running in a "desktop environment"/GUI, or if they are running in a terminal session.
                // Won't be able to launch a web browser without a GUI
                // https://en.wikipedia.org/wiki/Desktop_environment
                var currDesktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
                if (String.IsNullOrEmpty(currDesktopEnvironment))
                {
                    return;
                }

                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }
}