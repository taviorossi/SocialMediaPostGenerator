using Posts.Domain.Enums;

namespace Posts.Application.DTOs.Api;

/// <summary>
/// Response após submeter geração de vídeo.
/// </summary>
public class GenerateVideoResponse
{
    public Guid JobId { get; set; }
    public VideoJobStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}
