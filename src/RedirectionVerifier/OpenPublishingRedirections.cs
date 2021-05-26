using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace RedirectionVerifier
{
    internal sealed class OpenPublishingRedirections
    {
        [JsonPropertyName("redirections")]
        public ImmutableArray<Redirection> Redirections { get; set; }
    }
}
