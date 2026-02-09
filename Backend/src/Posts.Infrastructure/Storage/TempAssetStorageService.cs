using Microsoft.Extensions.Options;
using Posts.Application.Interfaces;

namespace Posts.Infrastructure.Storage;

/// <summary>
/// Salva imagens e áudios em diretório temporário e retorna URLs relativas ou absolutas.
/// Em produção, substituir por Cloud Storage e retornar URLs públicas.
/// </summary>
public class TempAssetStorageService : IAssetStorageService
{
    private readonly TempAssetStorageOptions _options;
    private readonly string _basePath;

    public TempAssetStorageService(IOptions<TempAssetStorageOptions> options)
    {
        _options = options.Value;
        _basePath = string.IsNullOrEmpty(_options.BasePath)
            ? Path.Combine(Path.GetTempPath(), "PostsAssets")
            : _options.BasePath;
        Directory.CreateDirectory(_basePath);
    }

    public Task<string> SaveImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_basePath, fileName);
        using var fs = File.Create(path);
        imageStream.CopyTo(fs);
        var url = _options.AssetsBaseUrl?.TrimEnd('/') != null
            ? $"{_options.AssetsBaseUrl.TrimEnd('/')}/api/assets/{fileName}"
            : path;
        return Task.FromResult(url);
    }

    public Task<string> SaveAudioAsync(Stream audioStream, string fileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_basePath, fileName);
        using var fs = File.Create(path);
        audioStream.CopyTo(fs);
        var url = _options.AssetsBaseUrl?.TrimEnd('/') != null
            ? $"{_options.AssetsBaseUrl.TrimEnd('/')}/api/assets/{fileName}"
            : path;
        return Task.FromResult(url);
    }
}

public class TempAssetStorageOptions
{
    public const string SectionName = "AssetStorage";
    public string? BasePath { get; set; }
    /// <summary>Base URL do backend para montar URLs acessíveis pela Shotstack (ex.: https://api.seudominio.com).</summary>
    public string? AssetsBaseUrl { get; set; }
}
