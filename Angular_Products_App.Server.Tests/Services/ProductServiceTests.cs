using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Repositories;
using MyApp.Services;
using Xunit;

namespace MyApp.Tests.Services
{
    public class ProductServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ProductRepository _repo;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _repo = new ProductRepository(_db);
            _service = new ProductService(_repo);
        }

        [Fact]
        public async Task CreateAsync_InvalidPrice_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(new Dtos.CreateProductDto { Name = "X", Price = -1m }));
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsDtoPagedResult()
        {
            for (int i = 1; i <= 15; i++)
            {
                await _repo.AddAsync(new Product { Name = $"P{i}", Price = i, Active = true });
            }

            var paged = await _service.GetPagedAsync(pageNumber: 2, pageSize: 5);
            Assert.Equal(5, paged.Items.Count());
            Assert.Equal(15, paged.TotalCount);
            Assert.Equal(2, paged.PageNumber);
        }

        [Fact]
        public async Task DeleteAsync_SetsInactive()
        {
            var created = await _repo.AddAsync(new Product { Name = "DeleteMe", Price = 2m, Active = true });
            var result = await _service.DeleteAsync(created.Id);
            Assert.True(result);

            var fetched = await _repo.GetByIdAsync(created.Id);
            Assert.False(fetched!.Active);
            Assert.NotNull(fetched.InActiveDate);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}