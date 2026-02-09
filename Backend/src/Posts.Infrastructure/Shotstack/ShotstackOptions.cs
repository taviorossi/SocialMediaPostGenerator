namespace Posts.Infrastructure.Shotstack;

public class ShotstackOptions
{
    public const string SectionName = "Shotstack";
    public string ApiKey { get; set; } = string.Empty;
    /// <summary>v1 (production) or stage (sandbox)</summary>
    public string Environment { get; set; } = "stage";
}
