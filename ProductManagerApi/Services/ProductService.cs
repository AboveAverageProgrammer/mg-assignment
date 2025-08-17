using ProductManagerApi.Dtos;
using ProductManagerApi.Entities;
using ProductManagerApi.Infrastructure;
using ProductManagerApi.Models;
using ProductManagerApi.Repositories;

namespace ProductManagerApi.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IProductUpdatePipe _pipe;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductService(IProductRepository repo, IProductUpdatePipe pipe, IServiceScopeFactory serviceScopeFactory)
    {
        _repo = repo;
        _pipe = pipe;
        _scopeFactory = serviceScopeFactory;
    }

    public Task<List<ProductListItem>> ListAsync(int limit, int offset, CancellationToken ct)
    {
        return _repo.ListAsync(limit, offset, ct);
    }

    public Task<Product?> GetAsync(int id, CancellationToken ct)
    {
        return _repo.GetAsync(id, ct);
    }

    public Task<CreateResult> AddAsync(CreateProduct dto, CancellationToken ct)
    {
        return _repo.AddAsync(dto, ct);
    }

    public async Task<UpdateResult> UpdateAsync(int id, UpdateProduct dto, uint expectedVersion, CancellationToken ct)
    {
        var code = UpdateResultCode.PreconditionFailed;
        uint? newVersion = null;

        var accepted = await _pipe.TryRunForProduct(id, async innerCt =>
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

            var updateResult = await repo.UpdateAsync(id, expectedVersion, dto, innerCt);
            code = updateResult.Code;
            newVersion = updateResult.NewVersion;
        }, timeout: TimeSpan.FromMilliseconds(250), ct);

        if (!accepted) return new UpdateResult(UpdateResultCode.PreconditionFailed, null); 
        return new UpdateResult(code, newVersion);
    }

    public async Task<DeleteResult> DeleteAsync(int id, uint expectedVersion, CancellationToken ct)
    {
        DeleteResult code = DeleteResult.PreconditionFailed;

        var accepted = await _pipe.TryRunForProduct(id, async innerCt =>
        {
            code = await _repo.DeleteAsync(id, expectedVersion, innerCt);
        }, timeout: TimeSpan.FromMilliseconds(250), ct);

        if (!accepted) return DeleteResult.PreconditionFailed; 
        return code;
    }
}