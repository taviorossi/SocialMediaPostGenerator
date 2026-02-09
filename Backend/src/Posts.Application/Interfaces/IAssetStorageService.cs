namespace Posts.Application.Interfaces;

/// <summary>
/// Serviço para persistir imagem e áudio e obter URLs para uso na timeline (ex.: Shotstack).
/// Implementação pode usar temp files, Cloud Storage, etc.
/// </summary>
public interface IAssetStorageService
{
    Task<string> SaveImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    Task<string> SaveAudioAsync(Stream audioStream, string fileName, CancellationToken cancellationToken = default);
}
