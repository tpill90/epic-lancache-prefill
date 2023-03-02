namespace EpicPrefill.Handlers
{
    //TODO document
    //TODO should probably have a private default constructor, so you have to use the Load() method
    /// <summary>
    /// https://dev.epicgames.com/docs/web-api-ref/authentication
    /// </summary>
    //TODO fix this warning
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Fix this.")]
    public sealed class UserAccountManager
    {
        private readonly IAnsiConsole _ansiConsole;

        //TODO comment properties
        private HttpClient _client;
        private string loginUrl = "https://legendary.gl/epiclogin";
        private string _oauth_host = "account-public-service-prod03.ol.epicgames.com";

        private string _user_basic = "34a02cf8f4414e29b15921876da36f9a";
        private string _pw_basic = "daafbccc737745039dffe53d94fc76cf";

        //TODO this should probably be private
        public OauthToken OauthToken { get; set; }

        public UserAccountManager(IAnsiConsole ansiConsole)
        {
            _ansiConsole = ansiConsole;
            _client = new HttpClient
            {
                Timeout = AppConfig.DefaultRequestTimeout
            };
            _client.DefaultRequestHeaders.Add("User-Agent", AppConfig.DefaultUserAgent);
        }

        //TODO document
        public async Task LoginAsync()
        {
            if (OauthToken != null && !IsOauthTokenExpired())
            {
                _ansiConsole.LogMarkupLine("Reusing existing auth session...");
                return;
            }

            var requestParams = new Dictionary<string, string>
            {
                { "token_type", "eg1" }
            };

            // Handles the user logging in for the first time
            if (OauthToken == null)
            {
                _ansiConsole.LogMarkupLine("Please login into Epic via your browser");
                //TODO which color to use for link?
                _ansiConsole.LogMarkupLine($"If the web page did not open automatically, please manually open the following URL: {Cyan(loginUrl)}");
                OpenUrl(loginUrl);

                //TODO handle users pasting in the entire JSON response.  Or figure out a way to not require doing this at all.
                //TODO might be able to do username + password without browser : https://gist.github.com/iXyles/ec40cb40a2a186425ec6bfb9dcc2ddda
                var authCode = _ansiConsole.Prompt(new TextPrompt<string>($"Please enter the {LightYellow("authorizationCode")} from the JSON response:"));

                requestParams.Add("grant_type", "authorization_code");
                requestParams.Add("code", authCode);
            }

            // Handles a user being logged in, but the saved token has expired
            if (OauthToken != null && IsOauthTokenExpired())
            {
                _ansiConsole.LogMarkupLine("Auth token expired.  Requesting refresh auth token...");

                requestParams.Add("grant_type", "refresh_token");
                requestParams.Add("refresh_token", OauthToken.RefreshToken);
            }


            var authUri = new Uri($"https://{_oauth_host}/account/api/oauth/token");
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, authUri);
            requestMessage.Content = new FormUrlEncodedContent(requestParams);

            var authenticationString = $"{_user_basic}:{_pw_basic}";
            var base64EncodedAuthenticationString = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            using var response = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            OauthToken = JsonSerializer.Deserialize(responseStream, SerializationContext.Default.OauthToken);
            Save();
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

        public void Save()
        {
            using var fileStream = File.Open(AppConfig.AccountSettingsStorePath, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(fileStream, OauthToken, SerializationContext.Default.OauthToken);
        }

        //TODO this likely won't work correctly from linux command line
        private void OpenUrl(string url)
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    Process.Start("xdg-open", url);
                }
                catch (Exception)
                {

                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }

        //TODO consider adding a buffer of 10ish minutes to the expired time, so that if a token expires while we're making requests, the requests won't fail
        private bool IsOauthTokenExpired()
        {
            // Tokens are valid for 8 hours
            return DateTimeOffset.UtcNow.DateTime > OauthToken.ExpiresAt;
        }
    }
}