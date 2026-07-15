using Microsoft.VisualBasic;
using Moq;
using MyApp.Dtos;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyApp.Repositories
{
    /// <summary>
    /// In-memory product repository for demo & tests. Implements the same behaviors:
    /// - paging/filtering
    /// - soft-delete (Active=false and InActiveDate set)
    /// - add/update
    /// - Mock repo is in-memory only and not persisted between runs.
	///     Mock repository behavior intentionally matches the real repository surface — paging, filtering,
    ///     soft-delete — so front-end and tests work unchanged. You can reuse MockProductRepository in
    ///     unit/integration tests by registering it in test DI or by calling it directly.
    /// </summary>
    public class MockProductRepository : IProductRepository
    {
        private readonly List<Product> _items = new();
        private int _idCounter;

        private readonly object _lock = new();

        public MockProductRepository()
        {
            SeedSampleData();
        }

        private void SeedSampleData()
        {
            // Sample demo data
            var samples = new[]
            {
                new Product { Name = "Acme Widget", Description = "Small widget", Price = 9.99m, Active = true },
                new Product { Name = "Acme Widget Pro", Description = "Pro widget with extras", Price = 19.99m, Active = true },
                new Product { Name = "Gizmo", Description = "Useful gizmo", Price = 14.50m, Active = true },
                new Product { Name = "Disposable Cup", Description = "Eco-friendly", Price = 1.50m, Active = true },
                new Product { Name = "Legacy Item", Description = "Old stock", Price = 0.99m, Active = false, InActiveDate = DateTime.UtcNow.AddDays(-30) },
                new Product { Name = "Demo Product A", Description = "Sample A", Price = 4.25m, Active = true },
                new Product { Name = "Demo Product B", Description = "Sample B", Price = 6.75m, Active = true },
                new Product { Name = "Seasonal Item", Description = "Limited", Price = 29.99m, Active = false, InActiveDate = DateTime.UtcNow.AddDays(-7) },
                new Product { Name = "Test Pack", Description = "Pack of 10", Price = 49.99m, Active = true },
                new Product { Name = "Sample Gift", Description = "Freebie", Price = 0m, Active = true }
            };

            lock (_lock)
            {
                foreach (var p in samples)
                {
                    p.Id = ++_idCounter;
                    _items.Add(p);
                }
            }
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                var p = _items.SingleOrDefault(x => x.Id == id);
                // return a shallow clone to avoid callers mutating internal list directly
                return Task.FromResult(p is null ? null : Clone(p));
            }
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            lock (_lock)
            {
                var list = _items.Where(p => p.Active).Select(Clone).ToList();
                return Task.FromResult<IEnumerable<Product>>(list);
            }
        }

        public Task<PagedResult<Product>> GetPagedAsync(int pageNumber = 1, int pageSize = 20, string? nameFilter = null, bool includeInactive = false, bool onlyInactive = false)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            lock (_lock)
            {
                IEnumerable<Product> q = _items.AsEnumerable();

                if (onlyInactive)
                    q = q.Where(p => !p.Active);
                else if (!includeInactive)
                    q = q.Where(p => p.Active);

                if (!string.IsNullOrWhiteSpace(nameFilter))
                    q = q.Where(p => p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));

                var total = q.Count();

                var items = q
                    .OrderBy(p => p.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(Clone)
                    .ToList();

                var result = new PagedResult<Product>(items, total, pageNumber, pageSize);
                return Task.FromResult(result);
            }
        }

        public Task<Product> AddAsync(Product product)
        {
            lock (_lock)
            {
                var clone = Clone(product);
                clone.Id = ++_idCounter;
                if (clone.InActiveDate != null) clone.Active = false;
                _items.Add(clone);
                return Task.FromResult(Clone(clone));
            }
        }

        public Task UpdateAsync(Product product)
        {
            lock (_lock)
            {
                var existing = _items.SingleOrDefault(x => x.Id == product.Id);
                if (existing == null) throw new InvalidOperationException("Product not found");
                existing.Name = product.Name;
                existing.Image = product.Image;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.Active = product.Active;
                existing.InActiveDate = product.InActiveDate;
                return Task.CompletedTask;
            }
        }

        public Task DeleteAsync(Product product)
        {
            lock (_lock)
            {
                var existing = _items.SingleOrDefault(x => x.Id == product.Id);
                if (existing != null)
                {
                    existing.Active = false;
                    existing.InActiveDate = DateTime.UtcNow;
                }
                return Task.CompletedTask;
            }
        }

        private static Product Clone(Product p) =>
            new Product
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