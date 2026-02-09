namespace Posts.Application.DTOs.Shotstack;

/// <summary>
/// Response do status do render (polling).
/// </summary>
public class ShotstackRenderStatusResponse
{
    public bool Success { get; set; }
    public string? Id { get; set; }
    public string Status { get; set; } = string.Empty; // fetching | rendering | saving | done | failed
    public string? Url { get; set; }
    public string? Error { get; set; }
}
