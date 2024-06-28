namespace EpicPrefill.Models.ApiResponses
{
    //TODO document
    //TODO rename
    public sealed class GameAsset
    {
        [JsonPropertyName("appName")]
        public string AppId { get; set; }

        [JsonPropertyName("buildVersion")]
        public string BuildVersion { get; set; }

        [JsonPropertyName("catalogItemId")]
        public string CatalogItemId { get; set; }

        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        [JsonIgnore]
        public string Title { get; set; }

        public override string ToString()
        {
            if (Title == null)
            {
                return $"{AppId} - {Namespace} - {CatalogItemId}";
            }
            return Title;
        }
    }
}