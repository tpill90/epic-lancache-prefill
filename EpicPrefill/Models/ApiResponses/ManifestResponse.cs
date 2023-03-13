using NStack;

namespace EpicPrefill.Models.ApiResponses
{
    //TODO document and figure out which fields arent needed
    public sealed class ManifestResponse
    {
        //TODO fix this warning.  Ignoring warning for the sake of releasing the app.  Build fails on warnings
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
        public Element[] elements { get; set; }
    }

    public class Element
    {
        //TODO fix this warning.  Ignoring warning for the sake of releasing the app.  Build fails on warnings
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
        public ManifestUrl[] manifests { get; set; }
    }

    public sealed class ManifestUrl
    {
        public Uri ManifestDownloadUri { get; set; }

        public string ManifestDownloadUrlWithParams => QueryHelpers.AddQueryString(ManifestDownloadUrl,
            queryParams.ToDictionary(e => e.name, e => e.value));

        private string _manifestDownloadUrl;
        /// <summary>
        /// Full download url for the manifest.  Shouldn't be used, as it will need additional parameters in order to work
        /// Example url:
        /// https://epicgames-download1.akamaized.net/Builds/Org/o-sdmmvl8pftrkwfy86bjp286kuwhnsq/0293ace10f2a46b481f736f3e06c491e/default/QMJA5gE-GnpknSNnF0CsiTveY-BBIw.manifest
        /// </summary>
        [JsonPropertyName("uri")]
        public string ManifestDownloadUrl
        {
            //TODO should probably be private
            get => _manifestDownloadUrl;
            set
            {
                _manifestDownloadUrl = value.Replace("https", "http");
                ManifestDownloadUri = new Uri(_manifestDownloadUrl);
            }
        }

        //TODO fix this warning.  Ignoring warning for the sake of releasing the app.  Build fails on warnings
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
        public Queryparam[] queryParams { get; set; }

        //TODO comment.  I've got no idea what this does anymore
        private string _baseUri;

        /// <summary>
        /// Gets the base url where the game assets can be downloaded.  Will be combined later to determine the full download url for the chunk.
        /// For example :
        /// https://epicgames-download1.akamaized.net/Builds/Org/o-sdmmvl8pftrkwfy86bjp286kuwhnsq/0293ace10f2a46b481f736f3e06c491e/default/QMJA5gE-GnpknSNnF0CsiTveY-BBIw.manifest
        /// will become:
        /// /Builds/Org/o-sdmmvl8pftrkwfy86bjp286kuwhnsq/0293ace10f2a46b481f736f3e06c491e/default/
        /// </summary>
        public string ChunkBaseUrl
        {
            get
            {
                if (_baseUri != null)
                {
                    return _baseUri;
                }

                var segments = ManifestDownloadUri.Segments.ToList();
                segments.RemoveAt(segments.Count - 1);
                _baseUri = Path.Join(segments.ToArray());
                return _baseUri;
            }
        }

        public override string ToString()
        {
            return ManifestDownloadUri.Host;
        }
    }

    public class Queryparam
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}