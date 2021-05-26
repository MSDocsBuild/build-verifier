using System.Text.Json.Serialization;

namespace RedirectionVerifier
{
    public sealed class Redirection
    {
        [JsonPropertyName("source_path")]
        public string SourcePath { get; set; } = null!;

        [JsonPropertyName("redirect_url")]
#pragma warning disable CA1056 // URI-like properties should not be strings → https://github.com/dotnet/roslyn-analyzers/issues/5133
        public string RedirectUrl { get; set; } = null!;
#pragma warning restore CA1056 // URI-like properties should not be strings
    }
}
