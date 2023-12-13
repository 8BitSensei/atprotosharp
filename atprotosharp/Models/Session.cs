using System.Text.Json.Serialization;

namespace atprotosharp.Models
{
    public struct Session
    {
        [JsonPropertyName("handle")]
        public string? Handle { get; set; }

        [JsonPropertyName("did")]
        public string? Did { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("accessJwt")]
        public string? AccessJwt { get; set; }

        public bool Success { get; set; }
    }
}
