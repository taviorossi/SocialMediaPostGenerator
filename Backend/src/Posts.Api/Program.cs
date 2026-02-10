using Microsoft.AspNetCore.Http.Features;
using Posts.Application.Interfaces;
using Posts.Application.Services;
using Posts.Api.Services;
using Posts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Carrega chaves locais (não versionado). Não commite appsettings.*.Local.json.
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.Local.json",
    optional: true,
    reloadOnChange: false);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<Posts.Api.Swagger.MultipartFormFileOperationFilter>();
    options.SchemaFilter<Posts.Api.Swagger.FormFileSchemaFilter>();
});

builder.Services.AddSingleton<IVideoJobStore, VideoJobStore>();
builder.Services.AddScoped<VideoOrchestratorService>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins.Length > 0 ? corsOrigins : ["*"])
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

// Redireciona a raiz para o Swagger para evitar 404 ao abrir a API no navegador
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();
