using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Posts.Api.Swagger;

/// <summary>
/// Substitui o schema de tipos que contêm IFormFile para evitar erro do Swagger ao gerar a spec.
/// </summary>
public class FormFileSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.GetProperties().All(p => p.PropertyType != typeof(IFormFile)))
            return;

        schema.Type = "object";
        schema.Properties = new Dictionary<string, OpenApiSchema>
        {
            ["image"] = new OpenApiSchema { Type = "string", Format = "binary", Description = "Arquivo de imagem (JPEG, PNG ou WebP)" },
            ["theme"] = new OpenApiSchema { Type = "string", Nullable = true },
            ["voiceId"] = new OpenApiSchema { Type = "string", Nullable = true }
        };
        schema.Required = new HashSet<string> { "image" };
    }
}
