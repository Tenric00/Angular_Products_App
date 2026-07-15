using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Dtos;
using MyApp.Models;
using MyApp.Repositories;

namespace MyApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            if (dto.Price < 0) throw new ArgumentException("Price cannot be negative.", nameof(dto.Price));

            var entity = new Product
            {
                Name = dto.Name,
                Image = dto.Image,
                Description = dto.Description,
                Price = dto.Price,
                Active = dto.Active
            };

            var created = await _repo.AddAsync(entity);
            return MapToDto(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            await _repo.DeleteAsync(existing); // soft-delete via repository
            return true;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto);
        }

        public async Task<PagedResult<ProductDto>> GetPagedAsync(int pageNumber = 1, int pageSize = 20, string? nameFilter = null, bool includeInactive = false, bool onlyInactive = false)
        {
            var paged = await _repo.GetPagedAsync(pageNumber, pageSize, nameFilter, includeInactive, onlyInactive);
            var dtoItems = paged.Items.Select(MapToDto);
            return new PagedResult<ProductDto>(dtoItems, paged.TotalCount, paged.PageNumber, paged.PageSize);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return p == null ? null : MapToDto(p);
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            if (dto.Price < 0) throw new ArgumentException("Price cannot be negative.", nameof(dto.Price));

            existing.Name = dto.Name;
            existing.Image = dto.Image;
            existing.Description = dto.Description;
            existing.Price = dto.Price;
            existing.Active = dto.Active;
            existing.InActiveDate = dto.InActiveDate;

            await _repo.UpdateAsync(existing);
            return true;
        }

        private static ProductDto MapToDto(Product p) =>
            new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Image = p.Image,
                Description = p.Description,
                Price = p.Price,
                Active = p.Active,
                InActiveDate = p.InActiveDate
            };
    }
}