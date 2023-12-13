using System.Text.Json.Serialization;

namespace atprotosharp.Models
{
    public class Timeline
    {
        [JsonPropertyName("feed")]
        public List<Post> Feed { get; set; }

    }

    public class Post 
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        [JsonPropertyName("cid")]
        public string Cid { get; set; }
        [JsonPropertyName("author")]
        public Profile Author { get; set; }
        [JsonPropertyName("record")]
        public Record Record { get; set; }
        [JsonPropertyName("replyCount")]
        public int ReplyCount { get; set; }
        [JsonPropertyName("repostCount")]
        public int RepostCount { get; set; }
        [JsonPropertyName("likeCount")]
        public int LikeCount { get; set; }
        [JsonPropertyName("indexedAt")]
        public DateTime IndexedAt { get; set; }
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; }
    }

    public class Record 
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("$type")]
        public string Type { get; set; }
        [JsonPropertyName("langs")]
        public List<string> Langs { get; set; }
        [JsonPropertyName("reply")]
        public Reply Reply { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

    }

    public class Reply 
    {
        [JsonPropertyName("root")]
        public ReplyRoot Root { get; set; }
        [JsonPropertyName("parent")]
        public ReplyRoot Parent { get; set; }
    }

    public struct ReplyRoot 
    {
        [JsonPropertyName("cid")]
        public string Cid { get; set; }
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }


}
