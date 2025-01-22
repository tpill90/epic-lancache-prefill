namespace EpicPrefill.Models
{
    // TODO comment
    public sealed class AppInfo
    {
        public string AppId { get; set; }
        public string BuildVersion { get; set; }
        public string CatalogItemId { get; set; }
        public string Namespace { get; set; }

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
