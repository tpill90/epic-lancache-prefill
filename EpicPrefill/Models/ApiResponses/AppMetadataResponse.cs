namespace EpicPrefill.Models.ApiResponses
{
    public class AppMetadataResponse
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }

        public override string ToString()
        {
            return title;
        }
    }
}