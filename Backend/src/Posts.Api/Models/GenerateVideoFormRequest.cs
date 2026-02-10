namespace Posts.Api.Models;

/// <summary>
/// Form multipart para geração de vídeo (imagem + tema + voiceId).
/// </summary>
public class GenerateVideoFormRequest
{
    /// <summary>Arquivo de imagem (JPEG, PNG ou WebP).</summary>
    public IFormFile? Image { get; set; }

    /// <summary>Tema ou contexto opcional para o roteiro.</summary>
    public string? Theme { get; set; }

    /// <summary>ID da voz para o áudio (opcional).</summary>
    public string? VoiceId { get; set; }
}
