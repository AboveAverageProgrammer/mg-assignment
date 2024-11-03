using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace ProductManagerApi.Middleware;

public class BasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the current endpoint requires authorization
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null || context.Request.Path.StartsWithSegments("/swagger"))
        {
            // Allow anonymous access; proceed to the next middleware
            await next(context);
            return;
        }

        // Check for Authorization header
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // Validate the Basic Auth credentials
        if (authHeader.Count > 0 && authHeader[0]!.StartsWith("Basic "))
        {
            var encodedCredentials = authHeader[0]?.Substring("Basic ".Length).Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials)).Split(':');

            if (credentials.Length == 2)
            {
                var username = credentials[0];
                var password = credentials[1];

                // Get expected credentials from configuration
                var expectedUsername = configuration["Auth:Username"] ?? "admin";
                var expectedPassword = configuration["Auth:Password"] ?? "admin";

                // Validate credentials
                if (username == expectedUsername && password == expectedPassword)
                {
                    await next(context); // Proceed to the next middleware
                    return;
                }
            }
        }
        
        // If we reach here, authentication failed
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }
}