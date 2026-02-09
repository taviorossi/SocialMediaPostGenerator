using Microsoft.AspNetCore.Http.Features;
using Posts.Application.Interfaces;
using Posts.Application.Services;
using Posts.Api.Services;
using Posts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapControllers();

app.Run();
