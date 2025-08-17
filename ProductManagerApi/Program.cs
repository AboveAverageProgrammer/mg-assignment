using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductManagerApi;
using ProductManagerApi.Infrastructure;
using ProductManagerApi.Middleware;
using ProductManagerApi.Repositories;
using ProductManagerApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add Basic Auth definition
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authentication header. Example: \"Basic {base64-encoded-username:password}\""
    });

    // Add global requirement for Basic Auth
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            Array.Empty<string>()
        }
    });

    options.OperationFilter<IfMatchHeaderOperationFilter>();
});
builder.Services.AddScoped(typeof(IProductRepository), typeof(ProductRepository));
builder.Services.AddScoped(typeof(IProductService), typeof(ProductService));
builder.Services.AddDbContext<ProductManagerApiContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("default"));
});

builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", _ => { });

builder.Services.AddSingleton<IProductUpdatePipe>(_ =>
    new ProductUpdatePipe(new ProductUpdatePipeOptions
    {
        DegreeOfParallelism = Environment.ProcessorCount,
        BoundedCapacity = 2048,
        MaxRetries = 1,
        RetryBaseDelay = TimeSpan.FromMilliseconds(150),
        IsTransient = ex => ex is TimeoutException
                         || ex is Npgsql.NpgsqlException { IsTransient: true }
                         || ex is Microsoft.EntityFrameworkCore.DbUpdateException
    }));

builder.Services.AddControllers();

var apiVersion = builder.Configuration["ApiVersion"] ?? "1";


builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(int.Parse(apiVersion), 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

var app = builder.Build();
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unhandled exception: {ex.Message}");

        var problemDetails = new ProblemDetails
        {
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred. Please try again later.",
            Instance = context.Request.Path
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductManagerApiContext>();
    db.Database.Migrate();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();