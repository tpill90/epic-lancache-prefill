namespace EpicPrefill.Models.Manifests
{
    public sealed class BinaryManifest
    {
        private readonly ulong _headerMagic = 0x44BEC00C;
        
        public Dictionary<string, CdnChunk> ChunkDataLookup;
        public ManifestUrl Url { get; private init; }
         
        public static BinaryManifest Parse(byte[] manifestBytes, ManifestUrl url)
        {
            var manifest = new BinaryManifest
            {
                Url = url
            };

            // Read header, and extract the manifest payload
            byte[] manifestPayload = manifest.ReadHeader(manifestBytes);

            // Parse the actual manifest
            using var stream = new MemoryStream(manifestPayload);
            using BinaryReader br = new BinaryReader(stream);

            // There is nothing that we need in the "metadata" section of the manifest.
            // The first 4 bytes represent the total size in bytes of the section, so we will simply skip ahead that many bytes
            int metadataSizeBytes = br.ReadInt32();
            // Need to account for the 4 bytes already read for the header size
            br.ReadBytes(metadataSizeBytes - 4);

            manifest.ChunkDataLookup = ParseChunkDataList(br);
            
            // Not reading FileManifest section, as well as the CustomFields section, as we don't use any of the data in those sections
            return manifest;
        }

        /// <summary>
        /// Reads the manifest header, and decompresses the actual manifest payload.
        /// </summary>
        /// <returns>The decompressed manifest payload, that can then be parsed.</returns>
        private byte[] ReadHeader(byte[] manifestBytes)
        {
            using var stream = new MemoryStream(manifestBytes);
            using BinaryReader br = new BinaryReader(stream);

            if (br.ReadUInt32() != _headerMagic)
            {
                throw new ManifestException("Error while reading manifest header, no header magic value found!");
            }

            uint headerSizeBytes = br.ReadUInt32();
            uint headerSizeUncompressed = br.ReadUInt32();
            uint headerSizeCompressed = br.ReadUInt32();

            // The expected SHA1 hash of the decompressed manifest payload
            string expectedShaHash = HexMate.Convert.ToHexString(br.ReadBytes(20), HexFormattingOptions.Lowercase);
            var isCompressed = br.ReadByte() == 1;
            var version = br.ReadUInt32();
            
            // Need to strip two bytes for the zlib header.  The built in dotnet decompression api does not make use of it.
            br.ReadBytes(2);

            var remainingByteCount = br.BaseStream.Length - br.BaseStream.Position;
            byte[] payload = br.ReadBytes((int)remainingByteCount);

            // Some manifests are uncompressed, however I haven't seen one yet
            if (!isCompressed)
            {
                return payload;
            }

            var decompressedPayload = payload.Decompress();
            var computedHash = decompressedPayload.ComputeSha1Hash().ToHexString();
            if (computedHash != expectedShaHash)
            {
                throw new ManifestException("Error while decompressing manifest.  Computed hash does not match expected hash!");
            }
            return decompressedPayload;
        }

        private static Dictionary<string, CdnChunk> ParseChunkDataList(BinaryReader br)
        {
            // The total bytes used for the ChunkDataList section of the manifest
            uint byteCount = br.ReadUInt32();
            byte version = br.ReadByte();
            int count = br.ReadInt32();

            var chunkInfos = new CdnChunk[count];

            // Guids
            byte[] sharedGuidBuffer = new byte[16];
            for (int i = 0; i < count; i++)
            {
                chunkInfos[i].Guid = br.ReadGuid(sharedGuidBuffer);
            }

            // Hash
            byte[] sharedLongBuffer = new byte[8];
            for (int i = 0; i < count; i++)
            {
                // Read the little endian 64 bit integer
                br.Read(sharedLongBuffer);
                // Reverse in place to be big endian
                Array.Reverse(sharedLongBuffer);

                chunkInfos[i].Hash = sharedLongBuffer.ToHexStringUpper();
            }
            
            // Skipping sha1's since we don't use them
            br.BaseStream.Seek(count * 20, SeekOrigin.Current);

            // Group number, is part of the download path
            for (int i = 0; i < count; i++)
            {
                chunkInfos[i].GroupNum = br.ReadByte();
            }

            // Uncompressed size
            for (int i = 0; i < count; i++)
            {
                chunkInfos[i].UncompressedSize = br.ReadUInt32();
            }

            // File size - the compressed size that will need to be downloaded
            for (var i = 0; i < count; i++)
            {
                chunkInfos[i].CompressedFileSize = br.ReadUInt64();
            }

            // Return as a lookup table
            return chunkInfos.ToDictionary(e => e.Guid, e => e);
        }
    }
}