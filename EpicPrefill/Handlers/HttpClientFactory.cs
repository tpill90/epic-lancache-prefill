namespace EpicPrefill.Handlers
{
    //TODO document
    public class HttpClientFactory
    {
        private readonly IAnsiConsole _ansiConsole;
        private readonly UserAccountManager _userAccountManager;

        public HttpClientFactory(IAnsiConsole ansiConsole, UserAccountManager userAccountManager)
        {
            _ansiConsole = ansiConsole;
            _userAccountManager = userAccountManager;
        }

        //TODO document
        public async Task<HttpClient> GetHttpClientAsync()
        {
            if (_userAccountManager.IsOauthTokenExpired())
            {
                await _userAccountManager.LoginAsync();
            }

            var client = new HttpClient
            {
                Timeout = AppConfig.DefaultRequestTimeout
            };
            client.DefaultRequestHeaders.Add("Authorization", $"bearer {_userAccountManager.OauthToken.AccessToken}");
            client.DefaultRequestHeaders.Add("User-Agent", AppConfig.DefaultUserAgent);
            return client;
        }
    }
}
