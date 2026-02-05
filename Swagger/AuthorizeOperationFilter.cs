using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Hosting;

namespace Api.Comercial.Swagger;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    private readonly IWebHostEnvironment _environment;

    public AuthorizeOperationFilter(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().Any() == true
            || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (!hasAuthorize)
        {
            return;
        }

        if (_environment.IsDevelopment())
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IOpenApiParameter>();
            }

            if (operation.Parameters.All(p => !string.Equals(p.Name, "dev_token", StringComparison.OrdinalIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "dev_token",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Token local de desenvolvimento (ex: local-dev-token).",
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                });
            }
        }
    }
}
