namespace Posts.Application.DTOs.Api;

/// <summary>
/// Request para iniciar geração de vídeo (imagem via multipart; tema opcional no controller).
/// </summary>
public class GenerateVideoRequest
{
    /// <summary>
    /// Tema ou contexto opcional para o roteiro (ex.: "promoção de Páscoa").
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Duração máxima desejada em segundos (opcional).
    /// </summary>
    public int? MaxDurationSeconds { get; set; }
}
