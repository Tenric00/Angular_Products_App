using System.Threading.Tasks;
using MyApp.Dtos;
using MyApp.Models;
using System.Collections.Generic;

namespace MyApp.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync(); // returns active items by default
        Task<PagedResult<Product>> GetPagedAsync(int pageNumber = 1, int pageSize = 20, string? nameFilter = null, bool includeInactive = false, bool onlyInactive = false);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product); // soft-delete
    }
}