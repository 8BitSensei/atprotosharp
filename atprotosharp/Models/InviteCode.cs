using System.Text.Json.Serialization;

namespace atprotosharp.Models
{
    public class InviteCode
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("available")]
        public int Available { get; set; }
        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; }
        [JsonPropertyName("forAccount")]
        public string ForAccount { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
        [JsonPropertyName("uses")]
        public Uses Uses { get; set; }
    }

    public struct Uses 
    {
        [JsonPropertyName("usedBy")]
        public string UsedBy { get; set; }
        [JsonPropertyName("usedAt")]
        public DateTime UsedAt { get; set; }
    }
}
