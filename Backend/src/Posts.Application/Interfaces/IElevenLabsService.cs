namespace Posts.Application.Interfaces;

/// <summary>
/// Serviço de síntese de voz via ElevenLabs API.
/// </summary>
public interface IElevenLabsService
{
    /// <summary>
    /// Gera áudio (fala) a partir do texto usando voice_id (inclui voz clonada).
    /// </summary>
    /// <param name="text">Texto a ser convertido em fala.</param>
    /// <param name="voiceId">ID da voz (null usa o padrão configurado).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Stream do áudio gerado (ex.: MP3).</returns>
    Task<Stream> GenerateSpeechAsync(string text, string? voiceId, CancellationToken cancellationToken = default);
}
