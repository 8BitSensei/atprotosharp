using System.Text.Json.Serialization;

namespace atprotosharp.Models
{
    public class Profile
    {
        [JsonPropertyName("Did")]
        public string? Did { get; set; }
        [JsonPropertyName("Handle")]
        public string? Handle { get; set; }
        [JsonPropertyName("DisplayName")]
        public string? DisplayName { get; set; }
        [JsonPropertyName("Avatar")]
        public string? Avatar { get; set; }
        [JsonPropertyName("Banner")]
        public string? Banner { get; set; }
        [JsonPropertyName("FollowsCount")]
        public int FollowsCount { get; set; }
        [JsonPropertyName("FollowersCount")]
        public int FollowersCount { get; set; }
        [JsonPropertyName("PostsCount")]
        public int PostsCount { get; set; }
        [JsonPropertyName("IndexedAt")]
        public string? IndexedAt { get; set; }
        [JsonPropertyName("Viewer")]
        public Viewer? Viewer { get; set; }
        [JsonPropertyName("Labels")]
        public List<string>? Labels { get; set; }
    }

    public class Viewer
    {
        [JsonPropertyName("Muted")]
        public bool Muted { get; set; }
        [JsonPropertyName("BlockedBy")]
        public bool BlockedBy { get; set; }
    }
}
