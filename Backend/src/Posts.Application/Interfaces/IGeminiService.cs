namespace Posts.Application.Interfaces;

/// <summary>
/// Serviço de geração de roteiro via Google Vertex AI (Gemini).
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Gera um roteiro para vídeo curto (TikTok/Reels) a partir da imagem e tema.
    /// </summary>
    Task<string> GenerateScriptAsync(Stream imageStream, string? theme, CancellationToken cancellationToken = default);
}
