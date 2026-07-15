using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyApp.Tests.Repositories
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ProductRepository _repo;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _repo = new ProductRepository(_db);
        }

        [Fact]
        public async Task AddAndGetById_ReturnsProduct()
        {
            var p = new Product { Name = "A", Price = 10m, Active = true };
            var created = await _repo.AddAsync(p);

            var fetched = await _repo.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.Equal("A", fetched!.Name);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsFilteredPagedResults()
        {
            for (int i = 1; i <= 25; i++)
            {
                await _repo.AddAsync(new Product { Name = "Product" + i, Price = i, Active = true });
            }

            var paged = await _repo.GetPagedAsync(pageNumber: 2, pageSize: 10);
            Assert.Equal(10, paged.Items.Count());
            Assert.Equal(25, paged.TotalCount);
            Assert.Equal(2, paged.PageNumber);
        }

        [Fact]
        public async Task DeleteAsync_SoftDeletesProduct()
        {
            var p = new Product { Name = "ToDelete", Price = 1m, Active = true };
            var created = await _repo.AddAsync(p);

            await _repo.DeleteAsync(created);

            var fetched = await _repo.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.False(fetched!.Active);
            Assert.NotNull(fetched.InActiveDate);
        }

        [Fact]
        public async Task GetPagedAsync_ExcludeInactiveByDefault()
        {
            var active = await _repo.AddAsync(new Product { Name = "Active", Price = 1m, Active = true });
            var inactive = await _repo.AddAsync(new Product { Name = "Inactive", Price = 2m, Active = true });
            await _repo.DeleteAsync(inactive);

            var paged = await _repo.GetPagedAsync(pageNumber: 1, pageSize: 10);
            Assert.Single(paged.Items);
            Assert.Equal(1, paged.TotalCount);
            Assert.Equal("Active", paged.Items.First().Name);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}