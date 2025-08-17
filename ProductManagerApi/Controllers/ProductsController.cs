using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagerApi.Dtos;
using ProductManagerApi.Entities;
using ProductManagerApi.Models;
using ProductManagerApi.Services;

namespace ProductManagerApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListItem>>> List([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
    {
        return Ok(await _productService.ListAsync(limit, offset, ct));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id, CancellationToken ct)
    {
        var product = await _productService.GetAsync(id, ct);

        if (product is null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        Response.Headers.ETag = product.Xmin.ToString();

        return Ok(product);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Basic")]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProduct product, CancellationToken ct)
    {
        var result = await _productService.AddAsync(product, ct);

        return result.Code switch
        {
            CreateResultCode.Created => CreatedAtAction(nameof(Create), new { id = result.Id }, result.Id),
            CreateResultCode.DuplicateName => Conflict("A product with the same name already exists."),
            _ => BadRequest("An unexpected error occurred while creating the product.")
        };
    }

    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "Basic")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProduct dto, CancellationToken ct)
    {
        var expectedVersion = await GetExpectedVersionAsync(ct);

        if (expectedVersion == 0)
        {
            return StatusCode(StatusCodes.Status412PreconditionFailed, "Precondition Required. Send If-Match with the version you read.");
        }

        var updateResult = await _productService.UpdateAsync(id, dto, expectedVersion, ct);

        if (updateResult.Code == UpdateResultCode.Ok && updateResult.NewVersion.HasValue)
        {
            Response.Headers.ETag = updateResult.NewVersion.Value.ToString();
        }

        return updateResult.Code switch
        {
            UpdateResultCode.Ok => NoContent(),
            UpdateResultCode.NotFound => NotFound(),
            UpdateResultCode.PreconditionFailed => StatusCode(StatusCodes.Status412PreconditionFailed, "Precondition Failed"),
            UpdateResultCode.DuplicateName => Conflict("Duplicate product name."),
            _ => BadRequest("An unexpected error occurred while updating the product.")
        };
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "Basic")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var expectedVersion = await GetExpectedVersionAsync(ct);

        if (expectedVersion == 0)
        {
            return StatusCode(StatusCodes.Status412PreconditionFailed, "Precondition Required. Send If-Match with the version you read.");
        }

        await _productService.DeleteAsync(id, expectedVersion, ct);
        return NoContent();
    }

    private Task<uint> GetExpectedVersionAsync(CancellationToken ct)
    {
        if (Request.Headers.TryGetValue("If-Match", out var etagRaw) && uint.TryParse(etagRaw.ToString().Trim('"'), out var expectedVersion))
        {
            return Task.FromResult(expectedVersion);
        }
        return Task.FromResult<uint>(0); 
    }
}
