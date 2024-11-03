using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ProductManagerApi;
using ProductManagerApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

app.UseHttpsRedirection();
apiGroup.MapGet("/public", [AllowAnonymous]() => Results.Ok("This is a public endpoint that does not require authentication."));

apiGroup.MapGet("/protected", () => Results.Ok("This is a protected endpoint that requires authentication."));

app.Run();