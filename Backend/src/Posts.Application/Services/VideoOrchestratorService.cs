using Microsoft.Extensions.Options;
using Posts.Application.DTOs.Api;
using Posts.Application.DTOs.Shotstack;
using Posts.Application.Interfaces;
using Posts.Domain.Enums;

namespace Posts.Application.Services;

/// <summary>
/// Orquestra o fluxo: Imagem -> Roteiro (Gemini) -> Áudio (ElevenLabs) -> Render (Shotstack).
/// </summary>
public class VideoOrchestratorService
{
    private readonly IGeminiService _geminiService;
    private readonly IElevenLabsService _elevenLabsService;
    private readonly IShotstackService _shotstackService;
    private readonly IAssetStorageService _assetStorage;
    private readonly IVideoJobStore _jobStore;
    private readonly VideoOrchestratorOptions _options;

    public VideoOrchestratorService(
        IGeminiService geminiService,
        IElevenLabsService elevenLabsService,
        IShotstackService shotstackService,
        IAssetStorageService assetStorage,
        IVideoJobStore jobStore,
        IOptions<VideoOrchestratorOptions> options)
    {
        _geminiService = geminiService;
        _elevenLabsService = elevenLabsService;
        _shotstackService = shotstackService;
        _assetStorage = assetStorage;
        _jobStore = jobStore;
        _options = options.Value;
    }

    /// <summary>
    /// Inicia a geração do vídeo: recebe imagem, gera roteiro, áudio e envia para renderização.
    /// Retorna imediatamente com JobId; o pipeline roda em background e o status é consultado via polling.
    /// </summary>
    public GenerateVideoResponse GenerateVideo(Stream imageStream, string? theme, string? voiceId = null)
    {
        var jobId = Guid.NewGuid();
        var state = new VideoJobState
        {
            JobId = jobId,
            Status = VideoJobStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _jobStore.Set(jobId, state);

        _ = RunPipelineAsync(jobId, imageStream, theme, voiceId);

        return new GenerateVideoResponse
        {
            JobId = jobId,
            Status = VideoJobStatus.Pending,
            Message = "Job submetido. Consulte o status em GET /api/video/{jobId}/status"
        };
    }

    private async Task RunPipelineAsync(Guid jobId, Stream imageStream, string? theme, string? voiceId)
    {
        try
        {
            UpdateJob(jobId, s => { s.Status = VideoJobStatus.Pending; s.UpdatedAt = DateTime.UtcNow; });

            using var imageCopy = new MemoryStream();
            await imageStream.CopyToAsync(imageCopy);
            imageCopy.Position = 0;

            // 1) Persistir imagem e obter URL para a timeline
            var imageUrl = await _assetStorage.SaveImageAsync(imageCopy, $"{jobId}.jpg");
            imageCopy.Position = 0;

            // 2) Gerar roteiro (Gemini)
            var script = await _geminiService.GenerateScriptAsync(imageCopy, theme);
            UpdateJob(jobId, s =>
            {
                s.Status = VideoJobStatus.ScriptGenerated;
                s.Script = script;
                s.UpdatedAt = DateTime.UtcNow;
            });

            // 3) Gerar áudio (ElevenLabs)
            using var audioStream = await _elevenLabsService.GenerateSpeechAsync(script, voiceId ?? _options.ElevenLabsVoiceId);
            var audioUrl = await _assetStorage.SaveAudioAsync(audioStream, $"{jobId}.mp3");
            UpdateJob(jobId, s =>
            {
                s.Status = VideoJobStatus.AudioGenerated;
                s.AudioUrl = audioUrl;
                s.UpdatedAt = DateTime.UtcNow;
            });

            // 4) Montar timeline e enviar para Shotstack
            var request = BuildShotstackRequest(imageUrl, audioUrl, script);
            var renderResponse = await _shotstackService.RenderAsync(request);
            if (!renderResponse.Success || string.IsNullOrEmpty(renderResponse.Id))
            {
                UpdateJob(jobId, s =>
                {
                    s.Status = VideoJobStatus.Failed;
                    s.ErrorMessage = renderResponse.Message ?? "Falha ao submeter render.";
                    s.UpdatedAt = DateTime.UtcNow;
                });
                return;
            }

            UpdateJob(jobId, s =>
            {
                s.Status = VideoJobStatus.RenderSubmitted;
                s.ShotstackRenderId = renderResponse.Id;
                s.UpdatedAt = DateTime.UtcNow;
            });

            // 5) Polling opcional até done (ou podemos deixar para o cliente fazer polling em Shotstack)
            var statusResponse = await _shotstackService.GetRenderStatusAsync(renderResponse.Id);
            if (statusResponse.Status == "done" && !string.IsNullOrEmpty(statusResponse.Url))
            {
                UpdateJob(jobId, s =>
                {
                    s.Status = VideoJobStatus.Done;
                    s.VideoUrl = statusResponse.Url;
                    s.UpdatedAt = DateTime.UtcNow;
                });
            }
            else if (statusResponse.Status == "failed")
            {
                UpdateJob(jobId, s =>
                {
                    s.Status = VideoJobStatus.Failed;
                    s.ErrorMessage = statusResponse.Error ?? "Render falhou.";
                    s.UpdatedAt = DateTime.UtcNow;
                });
            }
        }
        catch (Exception ex)
        {
            UpdateJob(jobId, s =>
            {
                s.Status = VideoJobStatus.Failed;
                s.ErrorMessage = ex.Message;
                s.UpdatedAt = DateTime.UtcNow;
            });
        }
    }

    private void UpdateJob(Guid jobId, Action<VideoJobState> update)
    {
        if (_jobStore.TryGet(jobId, out var state) && state != null)
        {
            update(state);
            _jobStore.Set(jobId, state);
        }
    }

    private static ShotstackTimelineRequest BuildShotstackRequest(string imageUrl, string audioUrl, string? titleText)
    {
        var tracks = new List<ShotstackTrack>();

        if (!string.IsNullOrWhiteSpace(titleText))
        {
            tracks.Add(new ShotstackTrack
            {
                Clips =
                [
                    new ShotstackClip
                    {
                        Asset = new ShotstackClipAsset { Type = "title", Text = titleText.Length > 80 ? titleText[..80] + "..." : titleText, Style = "minimal" },
                        Start = 0,
                        Length = 5
                    }
                ]
            });
        }

        tracks.Add(new ShotstackTrack
        {
            Clips =
            [
                new ShotstackClip
                {
                    Asset = new ShotstackClipAsset { Type = "image", Src = imageUrl },
                    Start = 0,
                    Length = 15
                }
            ]
        });
        tracks.Add(new ShotstackTrack
        {
            Clips =
            [
                new ShotstackClip
                {
                    Asset = new ShotstackClipAsset { Type = "audio", Src = audioUrl },
                    Start = 0,
                    Length = 15
                }
            ]
        });

        return new ShotstackTimelineRequest
        {
            Timeline = new ShotstackTimeline { Tracks = tracks },
            Output = new ShotstackOutput { Format = "mp4", Resolution = "sd" }
        };
    }
}

/// <summary>
/// Opções do orquestrador (ex.: voice_id padrão).
/// </summary>
public class VideoOrchestratorOptions
{
    public const string SectionName = "VideoOrchestrator";
    public string? ElevenLabsVoiceId { get; set; }
}
