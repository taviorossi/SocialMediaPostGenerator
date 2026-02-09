using Posts.Domain.Enums;

namespace Posts.Domain.Entities;

/// <summary>
/// Representa um job de geração de vídeo (para uso futuro com persistência).
/// </summary>
public class VideoJob
{
    public Guid Id { get; set; }
    public VideoJobStatus Status { get; set; }
    public string? Script { get; set; }
    public string? AudioUrl { get; set; }
    public string? ShotstackRenderId { get; set; }
    public string? VideoUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
