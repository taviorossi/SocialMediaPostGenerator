using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Posts.Api.Swagger;

/// <summary>
/// Configura operações com IFormFile como multipart/form-data no Swagger,
/// evitando SwaggerGeneratorException ao gerar a documentação.
/// </summary>
public class MultipartFormFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var paramDesc = context.ApiDescription.ParameterDescriptions
            .FirstOrDefault(p => p.Source?.Id == "Form");
        if (paramDesc?.Type == null || !HasFormFileProperty(paramDesc.Type))
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["image"] = new OpenApiSchema { Type = "string", Format = "binary", Description = "Arquivo de imagem (JPEG, PNG ou WebP)" },
                            ["theme"] = new OpenApiSchema { Type = "string", Nullable = true, Description = "Tema ou contexto opcional" },
                            ["voiceId"] = new OpenApiSchema { Type = "string", Nullable = true, Description = "ID da voz (opcional)" }
                        },
                        Required = new HashSet<string> { "image" }
                    }
                }
            }
        };

        operation.Parameters?.Clear();
    }

    private static bool HasFormFileProperty(Type type)
    {
        return type.GetProperties()
            .Any(p => p.PropertyType == typeof(IFormFile));
    }
}
