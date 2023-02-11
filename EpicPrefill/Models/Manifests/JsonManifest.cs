namespace EpicPrefill.Models.Manifests
{
    //TODO document
    //TODO can this be combined with the other manifest models
    public sealed class JsonManifest
    {
        public string ManifestFileVersion { get; set; }
        public ulong ManifestFileVersionNum;
        

        public Dictionary<string, string> ChunkHashList { get; set; }
        public Dictionary<string, string> ChunkShaList { get; set; }
        public Dictionary<string, string> DataGroupList { get; set; }
        public Dictionary<string, string> ChunkFilesizeList { get; set; }
        
        public string GetChunkDir()
        {
            if (ManifestFileVersionNum == 0)
            {
                ManifestFileVersionNum = ManifestFileVersion.BlobToNum();
            }
            //The lowest version I've ever seen was 12 (Unreal Tournament), but for completeness sake leave all of them in
            if (ManifestFileVersionNum >= 15)
            {
                return "ChunksV4";
            }
            if (ManifestFileVersionNum >= 6)
            {
                return "ChunksV3";
            }
            if (ManifestFileVersionNum >= 3)
            {
                return "ChunksV2";
            }
            return "Chunks";
        }
    }

    public class Filemanifestlist
    {
        public string Filename { get; set; }
        public string FileHash { get; set; }
        public Filechunkpart[] FileChunkParts { get; set; }
    }

    public class Filechunkpart
    {
        public string Guid { get; set; }
        public string Offset { get; set; }
        public string Size { get; set; }
    }
}