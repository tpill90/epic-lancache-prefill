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
        public string uri { get; set; }

        //TODO fix this warning.  Ignoring warning for the sake of releasing the app.  Build fails on warnings
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
        public Queryparam[] queryParams { get; set; }

        //TODO comment.  I've got no idea what this does anymore
        private string _baseUri;
        public string BaseUri
        {
            get
            {
                if (_baseUri != null)
                {
                    return _baseUri;
                }
                var uriTest = new Uri(uri);
                var segments = uriTest.Segments.ToList();
                segments.RemoveAt(segments.Count - 1);

                var newUri = new UriBuilder
                {
                    Scheme = uriTest.Scheme,
                    Host = uriTest.Host,
                    Path = Path.Join(segments.ToArray())
                };

                _baseUri = newUri.Uri.AbsoluteUri;
                return _baseUri;
            }
        }

        //TODO comment
        public string BasePath
        {
            get
            {
                var uriTest = new Uri(BaseUri);
                return uriTest.AbsolutePath;
            }
        }

        public string HostName => (new Uri(uri)).Host;

        public string UriWithParams
        {
            get
            {
                var parameters = queryParams.ToDictionary(e => e.name, e => e.value);
                return QueryHelpers.AddQueryString(uri.Replace("https", "http"), parameters);
            }
        }

        public override string ToString()
        {
            return HostName;
        }
    }

    public class Queryparam
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}