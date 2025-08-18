using ProductManagerApi.Dtos;
using ProductManagerApi.Entities;
using ProductManagerApi.Models;

namespace ProductManagerApi.Services;

public interface IProductService
{
    Task<List<ProductListItem>> ListAsync(int limit, int offset, CancellationToken ct);
    Task<Product?> GetAsync(int id, CancellationToken ct);
    Task<CreateResult> AddAsync(CreateProduct dto, CancellationToken ct);
    Task<UpdateResult> UpdateAsync(int id, UpdateProduct dto, uint expectedVersion, CancellationToken ct);
    Task<DeleteResult> DeleteAsync(int id, uint expectedVersion, CancellationToken ct);
}