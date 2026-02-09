using Posts.Application.DTOs.Api;

namespace Posts.Application.Interfaces;

/// <summary>
/// Store em memória dos jobs de vídeo (para status e polling).
/// </summary>
public interface IVideoJobStore
{
    void Set(Guid jobId, VideoJobState state);
    bool TryGet(Guid jobId, out VideoJobState? state);
}
