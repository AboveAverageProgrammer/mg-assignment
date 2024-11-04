using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using ProductManagerApi;
using ProductManagerApi.Entities;
using ProductManagerApi.Middleware;
using ProductManagerApi.Models;
using ProductManagerApi.Repositories;
using ProductManagerApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient(typeof(IProductRepository), typeof(ProductRepository));
builder.Services.AddTransient(typeof(IProductService), typeof(ProductService));
builder.Services.AddDbContext<ProductManagerApiContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("default"));
});
var app = builder.Build();
app.Use(async (context, next) =>
{
    try
    {
        await next(); // Proceed to the next middleware
    }
    catch (Exception ex)
    {
        // Log the exception (you can replace this with a logging framework)
        Console.WriteLine($"Unhandled exception: {ex.Message}");

        // Create a ProblemDetails response for the error
        var problemDetails = new ProblemDetails
        {
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred. Please try again later.",
            Instance = context.Request.Path
        };

        // Set response status code and content type
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
});

app.UseMiddleware<BasicAuthMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var apiGroup = app.MapGroup("/api/v1");
var productApiGroup = apiGroup.MapGroup("/product");

app.UseHttpsRedirection();
apiGroup.MapGet("/public",
    [AllowAnonymous]() => Results.Ok("This is a public endpoint that does not require authentication."));

apiGroup.MapGet("/protected", () => Results.Ok("This is a protected endpoint that requires authentication."));

productApiGroup.MapGet("/",
    [AllowAnonymous](IProductService productService) => Results.Ok(productService.GetProductList()));

productApiGroup.MapGet("/{id}", [AllowAnonymous]async(int id,IProductService productService) =>
{
    try
    {
        return Results.Ok(await productService.GetProductByIdAsync(id));
    }
    catch (EntityNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
});
productApiGroup.MapPost("/create", async([FromBody] Product product,IProductService productService) =>
{
    try
    {
        await productService.AddProductAsync(product);
        return Results.Created("/api/v1/product/create", product);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Product Name already exists");
    }
    catch (ValidationException e)
    {
        return Results.BadRequest(e.Message);
    }
});

app.Run();