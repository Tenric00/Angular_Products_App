using System.Threading.Tasks;
using MyApp.Dtos;
using System.Collections.Generic;

namespace MyApp.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<PagedResult<ProductDto>> GetPagedAsync(int pageNumber = 1, int pageSize = 20, string? nameFilter = null, bool includeInactive = false, bool onlyInactive = false);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<bool> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
    }
}