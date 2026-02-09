using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Posts.Application.Interfaces;

namespace Posts.Infrastructure.ElevenLabs;

/// <summary>
/// Síntese de voz via ElevenLabs API (inclui voice_id para voz clonada).
/// </summary>
public class ElevenLabsService : IElevenLabsService
{
    private readonly HttpClient _httpClient;
    private readonly ElevenLabsOptions _options;

    public ElevenLabsService(HttpClient httpClient, IOptions<ElevenLabsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<Stream> GenerateSpeechAsync(string text, string? voiceId, CancellationToken cancellationToken = default)
    {
        var id = voiceId ?? _options.VoiceId;
        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("ElevenLabs VoiceId não configurado.");

        var url = $"https://api.elevenlabs.io/v1/text-to-speech/{id}";
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("xi-api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "audio/mpeg");

        var body = new { text, model_id = _options.ModelId };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
