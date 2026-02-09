using Posts.Domain.Enums;

namespace Posts.Application.DTOs.Api;

/// <summary>
/// Response com status do job e URL do vídeo quando pronto.
/// </summary>
public class VideoJobStatusResponse
{
    public Guid JobId { get; set; }
    public VideoJobStatus Status { get; set; }
    public string? VideoUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ScriptGenerated { get; set; }
    public bool AudioGenerated { get; set; }
    public bool RenderSubmitted { get; set; }
}
