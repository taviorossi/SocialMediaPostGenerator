using System.Text.Json.Serialization;

namespace Posts.Application.DTOs.Shotstack;

/// <summary>
/// Request para a API Shotstack (POST /render). Body: timeline + output.
/// </summary>
public class ShotstackTimelineRequest
{
    [JsonPropertyName("timeline")]
    public ShotstackTimeline Timeline { get; set; } = new();

    [JsonPropertyName("output")]
    public ShotstackOutput? Output { get; set; }
}

public class ShotstackTimeline
{
    [JsonPropertyName("tracks")]
    public List<ShotstackTrack> Tracks { get; set; } = new();
}

public class ShotstackTrack
{
    [JsonPropertyName("clips")]
    public List<ShotstackClip> Clips { get; set; } = new();
}

public class ShotstackClip
{
    [JsonPropertyName("asset")]
    public ShotstackClipAsset Asset { get; set; } = new();

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("length")]
    public double Length { get; set; }
}

/// <summary>
/// Um clip tem um único asset (type + src para image/video/audio, ou type + text para title).
/// </summary>
public class ShotstackClipAsset
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "image"; // image | video | audio | title

    [JsonPropertyName("src")]
    public string? Src { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("style")]
    public string? Style { get; set; }
}

public class ShotstackOutput
{
    [JsonPropertyName("format")]
    public string Format { get; set; } = "mp4";

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; } = "sd"; // sd | hd

    [JsonPropertyName("fps")]
    public double? Fps { get; set; }
}
