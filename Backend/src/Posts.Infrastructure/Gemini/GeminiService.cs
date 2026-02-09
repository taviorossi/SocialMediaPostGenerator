using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Posts.Application.Interfaces;

namespace Posts.Infrastructure.Gemini;

/// <summary>
/// Geração de roteiro via Google Vertex AI (Gemini 1.5 Flash).
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly VertexAiOptions _options;

    public GeminiService(HttpClient httpClient, IOptions<VertexAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateScriptAsync(Stream imageStream, string? theme, CancellationToken cancellationToken = default)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = await CopyToByteArrayAsync(imageStream);
        var imageBase64 = Convert.ToBase64String(imageBytes);

        var prompt = BuildPrompt(theme);
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inlineData = new
                            {
                                mimeType = "image/jpeg",
                                data = imageBase64
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                maxOutputTokens = 1024,
                temperature = 0.7
            }
        };

        var url = $"https://{_options.Location}-aiplatform.googleapis.com/v1/projects/{_options.ProjectId}/locations/{_options.Location}/publishers/google/models/{_options.GeminiModel}:generateContent";
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonDocument.Parse(json);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
        return text ?? string.Empty;
    }

    private static string BuildPrompt(string? theme)
    {
        var themePart = string.IsNullOrWhiteSpace(theme) ? "" : $" Tema/foco: {theme}.";
        return "Você é um roteirista para vídeos curtos de marketing (TikTok/Reels). " +
               "Com base na imagem fornecida, gere um roteiro de narração em português brasileiro para um vídeo de até 30 segundos, " +
               "focado na marca Violeta & Cacau (doces) e em marketing de afiliados. " +
               "O texto deve ser natural para dublagem em voz. " +
               "Retorne apenas o texto do roteiro, sem instruções ou metadados." + themePart;
    }

    private static async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var credential = GoogleCredential.GetApplicationDefault();
        if (credential.IsCreateScopedRequired)
            credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await ((Google.Apis.Auth.OAuth2.ITokenAccess)credential).GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
    }

    private static async Task<byte[]> CopyToByteArrayAsync(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}
