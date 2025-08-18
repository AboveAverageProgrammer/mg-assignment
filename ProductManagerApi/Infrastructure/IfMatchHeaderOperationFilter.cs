using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProductManagerApi.Infrastructure;

public class IfMatchHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation == null || context == null)
            return;

        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();
        if (method == "PUT" || method == "DELETE")
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match",
                In = ParameterLocation.Header,
                Required = true,
                Description = "ETag value for optimistic concurrency. Example: \"12345\"",
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
