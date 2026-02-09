using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Posts.Infrastructure.Storage;

namespace Posts.Api.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetsController : ControllerBase
{
    private readonly string _basePath;

    public AssetsController(IOptions<TempAssetStorageOptions> options)
    {
        var opts = options.Value;
        _basePath = string.IsNullOrEmpty(opts.BasePath)
            ? Path.Combine(Path.GetTempPath(), "PostsAssets")
            : opts.BasePath;
    }

    /// <summary>
    /// Serve um asset (imagem ou áudio) por nome de arquivo para uso pela Shotstack.
    /// </summary>
    [HttpGet("{fileName}")]
    [ResponseCache(Duration = 0, NoStore = true)]
    public IActionResult Get(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || Path.IsPathRooted(fileName))
            return BadRequest();
        var path = Path.Combine(_basePath, fileName);
        if (!System.IO.File.Exists(path))
            return NotFound();
        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };
        return PhysicalFile(path, contentType, enableRangeProcessing: true);
    }
}
