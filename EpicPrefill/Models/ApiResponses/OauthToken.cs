namespace EpicPrefill.Models.ApiResponses
{
    //TODO document and figure out which fields arent needed
    public class OauthToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("refresh_expires_at")]
        public DateTime RefreshExpiresAt { get; set; }

        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }
}