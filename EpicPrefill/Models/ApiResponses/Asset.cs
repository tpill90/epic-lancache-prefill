namespace EpicPrefill.Models.ApiResponses
{
    //TODO document
    public sealed class Asset
    {
        [JsonPropertyName("appName")]
        public string AppId { get; set; }

        [JsonPropertyName("buildVersion")]
        public string BuildVersion { get; set; }

        [JsonPropertyName("catalogItemId")]
        public string CatalogItemId { get; set; }

        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        public override string ToString()
        {
            return $"{AppId} - {Namespace} - {CatalogItemId}";
        }
    }
}