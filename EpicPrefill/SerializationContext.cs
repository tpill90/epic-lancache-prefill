namespace EpicPrefill
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, WriteIndented = true)]
    [JsonSerializable(typeof(ManifestResponse))]
    [JsonSerializable(typeof(OauthToken))]
    [JsonSerializable(typeof(List<Asset>))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(Dictionary<string, AppMetadataResponse>))]
    [JsonSerializable(typeof(JsonManifest))]
    [JsonSerializable(typeof(Dictionary<string, HashSet<string>>))]
    internal sealed partial class SerializationContext : JsonSerializerContext
    {
    }
}