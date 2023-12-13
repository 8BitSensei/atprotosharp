using System.Text.Json.Serialization;

namespace atprotosharp;

public class AuthenticationRequest
{
    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}
