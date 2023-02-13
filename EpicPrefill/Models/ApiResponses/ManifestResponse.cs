namespace EpicPrefill.Models
{
    //TODO document and figure out which fields arent needed
    public class ManifestResponse
    {
        //TODO fix this warning.  Ignoring warning for the sake of releasing the app.  Build fails on warnings
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
        public Element[] elements { get; set; }
    }

    public class Element
    {
        public string appName { get; set; }
        public string labelName { get; set; }
        public string buildVersion { get; set; }
        public string hash { get; set; }
        public bool useSignedUrl { get; set; }

        //TODO fix this warning.  Ignoring warning for the sake of releasing the app.  Build fails on warnings
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
        public ManifestUrl[] manifests { get; set; }
    }

    public class ManifestUrl
    {
        public string uri { get; set; }

        //TODO fix this warning.  Ignoring warning for the sake of releasing the app.  Build fails on warnings
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
        public Queryparam[] queryParams { get; set; }

        //TODO comment
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

        public string UriWithParams
        {
            get
            {
                var parameters = queryParams.ToDictionary(e => e.name, e => e.value);
                return QueryHelpers.AddQueryString(uri, parameters);
            }
        }
    }

    public class Queryparam
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}