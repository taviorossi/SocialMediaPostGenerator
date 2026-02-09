using Posts.Domain.Enums;

namespace Posts.Application.DTOs.Api;

/// <summary>
/// Estado do job em memória (para consulta de status e polling).
/// </summary>
public class VideoJobState
{
    public Guid JobId { get; set; }
    public VideoJobStatus Status { get; set; }
    public string? Script { get; set; }
    public string? AudioUrl { get; set; }
    public string? ShotstackRenderId { get; set; }
    public string? VideoUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
