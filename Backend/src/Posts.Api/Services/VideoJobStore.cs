using System.Collections.Concurrent;
using Posts.Application.DTOs.Api;
using Posts.Application.Interfaces;

namespace Posts.Api.Services;

/// <summary>
/// Store em memória dos jobs (status para polling).
/// </summary>
public class VideoJobStore : IVideoJobStore
{
    private readonly ConcurrentDictionary<Guid, VideoJobState> _store = new();

    public void Set(Guid jobId, VideoJobState state) => _store[jobId] = state;

    public bool TryGet(Guid jobId, out VideoJobState? state) => _store.TryGetValue(jobId, out state);
}
