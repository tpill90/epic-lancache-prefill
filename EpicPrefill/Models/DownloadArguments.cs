namespace EpicPrefill.Models
{
    public sealed class DownloadArguments
    {
        /// <summary>
        /// When set to true, always run the download, regardless of if the app has been previously downloaded.
        /// </summary>
        public bool Force { get; init; }

        /// <summary>
        /// Determines which unit to display the download speed in.
        /// </summary>
        public TransferSpeedUnit TransferSpeedUnit { get; set; } = TransferSpeedUnit.Bits;
    }
}
