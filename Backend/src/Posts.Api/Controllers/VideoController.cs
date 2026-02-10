using Microsoft.AspNetCore.Mvc;
using Posts.Api.Models;
using Posts.Application.DTOs.Api;
using Posts.Application.Interfaces;
using Posts.Application.Services;
using Posts.Domain.Enums;

namespace Posts.Api.Controllers;

[ApiController]
[Route("api/video")]
public class VideoController : ControllerBase
{
    private readonly VideoOrchestratorService _orchestrator;
    private readonly IVideoJobStore _jobStore;

    public VideoController(VideoOrchestratorService orchestrator, IVideoJobStore jobStore)
    {
        _orchestrator = orchestrator;
        _jobStore = jobStore;
    }

    /// <summary>
    /// Envia uma imagem para iniciar a geração do vídeo (roteiro -> áudio -> render).
    /// Retorna JobId para consulta de status.
    /// </summary>
    [HttpPost("generate")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Generate([FromForm] GenerateVideoFormRequest request)
    {
        var image = request?.Image;
        if (image == null || image.Length == 0)
            return BadRequest("Envie um arquivo de imagem (multipart/form-data, key: image).");

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(image.ContentType))
            return BadRequest("Tipo de arquivo não suportado. Use JPEG, PNG ou WebP.");

        using var stream = image.OpenReadStream();
        var response = _orchestrator.GenerateVideo(stream, request.Theme, request.VoiceId);
        return Ok(response);
    }

    /// <summary>
    /// Retorna o status do job e a URL do vídeo quando pronto.
    /// </summary>
    [HttpGet("{jobId:guid}/status")]
    [ProducesResponseType(typeof(VideoJobStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetStatus(Guid jobId)
    {
        if (!_jobStore.TryGet(jobId, out var state) || state == null)
            return NotFound();

        return Ok(new VideoJobStatusResponse
        {
            JobId = state.JobId,
            Status = state.Status,
            VideoUrl = state.VideoUrl,
            ErrorMessage = state.ErrorMessage,
            ScriptGenerated = state.Status >= VideoJobStatus.ScriptGenerated,
            AudioGenerated = state.Status >= VideoJobStatus.AudioGenerated,
            RenderSubmitted = state.Status >= VideoJobStatus.RenderSubmitted
        });
    }
}
