using System.Text.Json.Serialization;

namespace atprotosharp.Models
{
    public class Profile
    {
        [JsonPropertyName("did")]
        public string? Did { get; set; }
        [JsonPropertyName("handle")]
        public string? Handle { get; set; }
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
        [JsonPropertyName("banner")]
        public string? Banner { get; set; }
        [JsonPropertyName("followsCount")]
        public int FollowsCount { get; set; }
        [JsonPropertyName("followersCount")]
        public int FollowersCount { get; set; }
        [JsonPropertyName("postsCount")]
        public int PostsCount { get; set; }
        [JsonPropertyName("indexedAt")]
        public string? IndexedAt { get; set; }
        [JsonPropertyName("viewer")]
        public Viewer? Viewer { get; set; }
        [JsonPropertyName("labels")]
        public List<string>? Labels { get; set; }
    }

    public struct Viewer
    {
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
        [JsonPropertyName("blockedBy")]
        public bool BlockedBy { get; set; }
    }
}
