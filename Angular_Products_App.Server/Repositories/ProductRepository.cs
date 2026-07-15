using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

namespace MyApp.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Product> AddAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(Product product)
        {
            // Soft-delete: mark inactive and set InActiveDate
            product.Active = false;
            product.InActiveDate = DateTime.UtcNow;
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            // Return only active items by default
            return await _db.Products.AsNoTracking().Where(p => p.Active).ToListAsync();
        }

        public async Task<PagedResult<Product>> GetPagedAsync(int pageNumber = 1, int pageSize = 20, string? nameFilter = null, bool includeInactive = false, bool onlyInactive = false)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            // Build the query with optional filters. AsQueryable is used to allow further filtering and pagination.
            IQueryable<Product> query = _db.Products.AsQueryable();

            if (onlyInactive)
            {
                query = query.Where(p => !p.Active);
            }
            else if (!includeInactive)
            {
                query = query.Where(p => p.Active);
            }

            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                query = query.Where(p => p.Name.Contains(nameFilter));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Product>(items, total, pageNumber, pageSize);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.Products.FindAsync(id);
        }

        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }
    }
}