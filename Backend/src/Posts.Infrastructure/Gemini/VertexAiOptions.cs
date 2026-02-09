namespace Posts.Infrastructure.Gemini;

public class VertexAiOptions
{
    public const string SectionName = "VertexAI";
    public string ProjectId { get; set; } = string.Empty;
    public string Location { get; set; } = "us-central1";
    public string GeminiModel { get; set; } = "gemini-1.5-flash";
}
