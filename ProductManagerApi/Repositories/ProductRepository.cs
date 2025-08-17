using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using ProductManagerApi.Dtos;
using ProductManagerApi.Entities;
using ProductManagerApi.Models;

namespace ProductManagerApi.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductManagerApiContext _context;
    public ProductRepository(ProductManagerApiContext context)
    {
        _context = context;
    }

    public Task<List<ProductListItem>> ListAsync(int limit, int offset, CancellationToken ct)
    {
        return _context.Products.AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip(offset)
            .Take(limit)
            .Select(p => new ProductListItem(p.Id, p.Name, p.Available, p.Price))
            .ToListAsync(ct);
    }

    public Task<Product?> GetAsync(int id, CancellationToken ct)
    {
        return _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<CreateResult> AddAsync(CreateProduct dto, CancellationToken ct)
    {
        var rows = await _context.Database.SqlQuery<int>($@"
            INSERT INTO products (name, price, available, description, date_created)
            VALUES ({dto.Name}, {dto.Price}, {dto.Available}, {dto.Description}, {DateTime.UtcNow})
            ON CONFLICT (Name) DO NOTHING
            RETURNING id;
        ").ToListAsync(ct);

        return rows.Count == 1
            ? new CreateResult(CreateResultCode.Created, rows[0])
            : new CreateResult(CreateResultCode.DuplicateName, null);
    }

    public async Task<UpdateResult> UpdateAsync(int id, uint expectedVersion, UpdateProduct dto, CancellationToken ct)
    {
        var nameTaken = await _context.Products.AsNoTracking()
            .AnyAsync(p => p.Name == dto.Name && p.Id != id, ct);
        if (nameTaken) return new UpdateResult(UpdateResultCode.DuplicateName, null);

        try
        {
            var sql = @" UPDATE products
                        SET name = @name, price = @price, available = @available, description = @description
                        WHERE id = @id
                        AND xmin = @expected               
                        RETURNING xmin::text AS version";

            var versions = await _context.Database.SqlQueryRaw<string>(
                sql,
                new NpgsqlParameter<string>("name", dto.Name),
                new NpgsqlParameter<decimal>("price", dto.Price),
                new NpgsqlParameter<bool>("available", dto.Available),
                new NpgsqlParameter<string?>("description", dto.Description),
                new NpgsqlParameter<int>("id", id),
                new NpgsqlParameter<uint>("expected", expectedVersion)
                {
                    NpgsqlDbType = NpgsqlDbType.Xid
                })
                .ToListAsync(ct);

            if (versions.Count == 1)
            {
                return new UpdateResult(UpdateResultCode.Ok, uint.Parse(versions[0]));
            }

            var exists = await _context.Products.AsNoTracking().AnyAsync(p => p.Id == id, ct);
            return new UpdateResult(exists ? UpdateResultCode.PreconditionFailed : UpdateResultCode.NotFound, null);
        }
        catch (Npgsql.NpgsqlException ex) when (ex.SqlState == "23505")
        {
            return new UpdateResult(UpdateResultCode.DuplicateName, null);
        }
        catch (Exception e)
        {
            return new UpdateResult(UpdateResultCode.NotFound, null);
        }
    }

    public async Task<DeleteResult> DeleteAsync(int id, uint expectedVersion, CancellationToken ct)
    {
        var rows = await _context.Database.SqlQueryRaw<int>($@"
            DELETE FROM products
             WHERE id = @id
                AND xmin = @xmin 
             RETURNING id;
        ", 
        new NpgsqlParameter<int>("id", id),
        new NpgsqlParameter<uint>("xmin", expectedVersion) 
        { 
            NpgsqlDbType =  NpgsqlDbType.Xid 
        }).ToListAsync(ct);

        if (rows.Count == 1)
        {
            return DeleteResult.Ok;
        }

        var exists = await _context.Products.AsNoTracking().AnyAsync(p => p.Id == id, ct);
        return exists ? DeleteResult.PreconditionFailed : DeleteResult.NotFound;
    }
}