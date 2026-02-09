namespace Posts.Application.DTOs.Shotstack;

/// <summary>
/// Response do submit de render da Shotstack.
/// </summary>
public class ShotstackRenderResponse
{
    public bool Success { get; set; }
    public string? Id { get; set; }
    public string? Message { get; set; }
}
