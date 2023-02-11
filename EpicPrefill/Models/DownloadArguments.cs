namespace EpicPrefill.Models
{
    public sealed class DownloadArguments
    {
        /// <summary>
        /// When set to true, always run the download, regardless of if the app has been previously downloaded.
        /// </summary>
        public bool Force { get; init; }

        /// <summary>
        /// When set to true, will avoid saving as much data to disk as possible
        /// </summary>
        public bool NoCache { get; set; }

        /// <summary>
        /// Determines which unit to display the download speed in.
        /// </summary>
        public TransferSpeedUnit TransferSpeedUnit { get; set; } = TransferSpeedUnit.Bits;

        //TODO comment + implement.  Should maybe not be in the app config?
        public int MaxConcurrentRequests { get; set; } = 30;
    }
}
