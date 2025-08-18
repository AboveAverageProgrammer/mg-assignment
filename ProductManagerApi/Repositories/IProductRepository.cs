using ProductManagerApi.Dtos;
using ProductManagerApi.Entities;
using ProductManagerApi.Models;

namespace ProductManagerApi.Repositories;

public interface IProductRepository
{
    Task<List<ProductListItem>> ListAsync(int limit, int offset, CancellationToken cancellationToken);
    Task<Product?> GetAsync(int id, CancellationToken cancellationToken);
    Task<CreateResult> AddAsync(CreateProduct dto, CancellationToken cancellationToken);
    Task<UpdateResult> UpdateAsync(int id, uint expectedVersion, UpdateProduct dto, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(int id, uint expectedVersion, CancellationToken cancellationToken);
}