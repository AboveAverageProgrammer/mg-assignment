using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProductManagerApi;
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
    [AllowAnonymous](IProductService productService) => Results.Ok(productService.GetProductListAsync()));

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
    catch (Exception e)
    {
        Console.WriteLine(e);
        return Results.BadRequest(e.ToString());
    }
});

app.Run();