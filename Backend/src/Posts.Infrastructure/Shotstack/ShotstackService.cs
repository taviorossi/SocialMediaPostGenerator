using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Posts.Application.DTOs.Shotstack;
using Posts.Application.Interfaces;

namespace Posts.Infrastructure.Shotstack;

/// <summary>
/// Renderização de vídeo via Shotstack API.
/// </summary>
public class ShotstackService : IShotstackService
{
    private readonly HttpClient _httpClient;
    private readonly ShotstackOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ShotstackService(HttpClient httpClient, IOptions<ShotstackOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
    }

    public async Task<ShotstackRenderResponse> RenderAsync(ShotstackTimelineRequest request, CancellationToken cancellationToken = default)
    {
        var version = _options.Environment.Equals("v1", StringComparison.OrdinalIgnoreCase) ? "v1" : "stage";
        var url = $"https://api.shotstack.io/edit/{version}/render";
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return new ShotstackRenderResponse { Success = false, Message = responseJson };

        var doc = JsonDocument.Parse(responseJson);
        var success = doc.RootElement.TryGetProperty("success", out var s) && s.GetBoolean();
        var id = doc.RootElement.TryGetProperty("response", out var resp) && resp.TryGetProperty("id", out var idEl)
            ? idEl.GetString()
            : null;
        var message = doc.RootElement.TryGetProperty("response", out var r2) && r2.TryGetProperty("message", out var msg)
            ? msg.GetString()
            : null;
        return new ShotstackRenderResponse { Success = success, Id = id, Message = message };
    }

    public async Task<ShotstackRenderStatusResponse> GetRenderStatusAsync(string renderId, CancellationToken cancellationToken = default)
    {
        var version = _options.Environment.Equals("v1", StringComparison.OrdinalIgnoreCase) ? "v1" : "stage";
        var url = $"https://api.shotstack.io/edit/{version}/render/{renderId}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return new ShotstackRenderStatusResponse { Success = false, Error = responseJson };

        var doc = JsonDocument.Parse(responseJson);
        var success = doc.RootElement.TryGetProperty("success", out var s) && s.GetBoolean();
        if (!doc.RootElement.TryGetProperty("response", out var resp))
            return new ShotstackRenderStatusResponse { Success = success };

        var status = resp.TryGetProperty("status", out var st) ? st.GetString() ?? "" : "";
        var urlVal = resp.TryGetProperty("url", out var u) ? u.GetString() : null;
        var id = resp.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        var err = resp.TryGetProperty("error", out var errEl) ? errEl.GetString() : null;
        return new ShotstackRenderStatusResponse
        {
            Success = success,
            Id = id,
            Status = status ?? "",
            Url = urlVal,
            Error = err
        };
    }
}
