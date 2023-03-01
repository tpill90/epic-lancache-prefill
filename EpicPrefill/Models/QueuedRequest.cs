namespace EpicPrefill.Models
{
    //TODO document
    public sealed class QueuedRequest
    {
        public string DownloadUrl { get; set; }

        public ulong DownloadSizeBytes { get; set; }
    }
}