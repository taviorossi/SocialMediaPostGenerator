using Posts.Application.DTOs.Shotstack;

namespace Posts.Application.Interfaces;

/// <summary>
/// Serviço de renderização de vídeo via Shotstack API.
/// </summary>
public interface IShotstackService
{
    /// <summary>
    /// Envia a timeline para renderização e retorna o id do render.
    /// </summary>
    Task<ShotstackRenderResponse> RenderAsync(ShotstackTimelineRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o status e URL do vídeo quando pronto.
    /// </summary>
    Task<ShotstackRenderStatusResponse> GetRenderStatusAsync(string renderId, CancellationToken cancellationToken = default);
}
