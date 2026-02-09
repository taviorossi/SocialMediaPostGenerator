using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Posts.Application.Interfaces;
using Posts.Application.Services;
using Posts.Infrastructure.ElevenLabs;
using Posts.Infrastructure.Gemini;
using Posts.Infrastructure.Shotstack;
using Posts.Infrastructure.Storage;

namespace Posts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<VertexAiOptions>(configuration.GetSection(VertexAiOptions.SectionName));
        services.Configure<ElevenLabsOptions>(configuration.GetSection(ElevenLabsOptions.SectionName));
        services.Configure<ShotstackOptions>(configuration.GetSection(ShotstackOptions.SectionName));
        services.Configure<TempAssetStorageOptions>(configuration.GetSection(TempAssetStorageOptions.SectionName));
        services.Configure<VideoOrchestratorOptions>(configuration.GetSection(VideoOrchestratorOptions.SectionName));

        services.AddHttpClient<IGeminiService, GeminiService>();
        services.AddHttpClient<IElevenLabsService, ElevenLabsService>();
        services.AddHttpClient<IShotstackService, ShotstackService>();
        services.AddSingleton<IAssetStorageService, TempAssetStorageService>();

        return services;
    }
}
