namespace Posts.Domain.Enums;

/// <summary>
/// Status do job de geração de vídeo ao longo do pipeline.
/// </summary>
public enum VideoJobStatus
{
    Pending = 0,
    ScriptGenerated = 1,
    AudioGenerated = 2,
    RenderSubmitted = 3,
    Done = 4,
    Failed = 5
}
