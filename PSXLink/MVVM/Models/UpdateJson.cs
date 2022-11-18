using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PSXLink.MVVM.Models
{
    public class UpdateJson
    {
        [JsonPropertyName("originalFileSize")]
        public long OriginalFileSize { get; set; }
        [JsonPropertyName("packageDigest")]
        public string? PackageDigest { get; set; }
        [JsonPropertyName("numberOfSplitFiles")]
        public int NumberOfSplitFiles { get; set; }
        [JsonPropertyName("playgoChunkCrcHashValue")]
        public string? PlaygoChunkCrcHashValue { get; set; }
        [JsonPropertyName("playgoChunkCrcUrl")]
        public string? PlaygoChunkCrcUrl { get; set; }
        [JsonPropertyName("pieces")]
        public List<Piece>? Pieces { get; set; }
    }

    public class Piece
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("fileOffset")]
        public long FileOffset { get; set; }
        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }
        [JsonPropertyName("hashValue")]
        public string? HashValue { get; set; }
    }
}
