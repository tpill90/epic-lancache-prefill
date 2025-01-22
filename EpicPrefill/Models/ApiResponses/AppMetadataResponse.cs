namespace EpicPrefill.Models.ApiResponses
{
    public sealed class AppMetadataResponse
    {
        [JsonPropertyName("id")]
        public string CatalogItemId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}