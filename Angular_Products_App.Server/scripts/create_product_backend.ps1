<#
.SYNOPSIS
Creates Product backend files (EF Core entity, DbContext, DTOs, repository, service, controller, Program and appsettings snippet)
into the Angular_Products_App.Server folder.

.PARAMETER Base
Root backend path. Default matches your workspace.

.PARAMETER InstallPackages
If specified, runs `dotnet add package` for EF Core packages against the found .csproj.

.PARAMETER RunMigrations
If specified, runs `dotnet ef migrations add InitialCreate` and `dotnet ef database update`.
Requires `dotnet-ef` available and `InstallPackages` or packages already present.

.EXAMPLE
.\create_product_backend.ps1 -InstallPackages
#>

param(
    [string]$Base = 'C:\Users\avery\Source\Repos\Angular_Products_App\Angular_Products_App.Server',
    [switch]$InstallPackages,
    [switch]$RunMigrations
)

$ErrorActionPreference = 'Stop'

# Files to create (relative paths)
$files = @{
    'Models\Product.cs' = @'
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(2048)]
        public string? Image { get; set; }

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool Active { get; set; } = true;

        public DateTime? InActiveDate { get; set; }
    }
}
'@

    'Data\ApplicationDbContext.cs' = @'
using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Image)
                      .HasMaxLength(2048);

                entity.Property(e => e.Description)
                      .HasMaxLength(4000);

                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(e => e.Active)
                      .HasDefaultValue(true);

                entity.Property(e => e.InActiveDate)
                      .IsRequired(false);

                entity.HasIndex(e => e.Name);
            });
        }
    }
}
'@

    'Dtos\ProductDto.cs' = @'
using System;

namespace MyApp.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Image { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool Active { get; set; }
        public DateTime? InActiveDate { get; set; }
    }
}
'@

    'Dtos\CreateProductDto.cs' = @'
using System.ComponentModel.DataAnnotations;

namespace MyApp.Dtos
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(2048)]
        public string? Image { get; set; }

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool Active { get; set; } = true;
    }
}
'@

    'Dtos\UpdateProductDto.cs' = @'
using System;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Dtos
{
    public class UpdateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(2048)]
        public string? Image { get; set; }

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool Active { get; set; }

        public DateTime? InActiveDate { get; set; }
    }
}
'@

    'Repositories\IProductRepository.cs' = @'
using System.Collections.Generic;
using System.Threading.Tasks;
using MyApp.Models;

namespace MyApp.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
    }
}
'@

    'Repositories\ProductRepository.cs' = @'
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
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
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _db.Products.AsNoTracking().ToListAsync();
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
'@

    'Services\IProductService.cs' = @'
using System.Collections.Generic;
using System.Threading.Tasks;
using MyApp.Dtos;

namespace MyApp.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<bool> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
'@

    'Services\ProductService.cs' = @'
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
            await _repo.DeleteAsync(existing);
            return true;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto);
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
'@

    'Controllers\ProductsController.cs' = @'
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Dtos;
using MyApp.Services;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _svc;

        public ProductsController(IProductService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _svc.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _svc.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ok = await _svc.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
'@

    'Program.cs' = @'
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Repositories;
using MyApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Update connection string in appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
'@

    'appsettings.json' = @'
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AngularProductsAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": { "Default": "Information" }
  }
}
'@
}

# Create files
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
foreach ($rel in $files.Keys) {
    $fullPath = Join-Path $Base $rel
    $dir = Split-Path $fullPath -Parent
    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    }

    if (Test-Path $fullPath) {
        $bak = "$fullPath.bak.$timestamp"
        Copy-Item -Path $fullPath -Destination $bak -Force
        Write-Host "Backed up existing file to: $bak"
    }

    $content = $files[$rel]
    $content | Out-File -FilePath $fullPath -Encoding utf8 -Force
    Write-Host "Written: $fullPath"
}

# Optionally install packages
if ($InstallPackages) {
    $csproj = Get-ChildItem -Path $Base -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $csproj) {
        Write-Warning "No .csproj found under $Base. Skipping package installation."
    } else {
        $projPath = $csproj.FullName
        Write-Host "Using project: $projPath"
        $packages = @(
            'Microsoft.EntityFrameworkCore.SqlServer',
            'Microsoft.EntityFrameworkCore.Design',
            'Microsoft.EntityFrameworkCore.Tools'
        )
        foreach ($pkg in $packages) {
            Write-Host "Adding package $pkg..."
            & dotnet add $projPath package $pkg
        }
    }
}

# Optionally run migrations
if ($RunMigrations) {
    $csproj = Get-ChildItem -Path $Base -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $csproj) {
        Write-Warning "No .csproj found under $Base. Skipping migrations."
    } else {
        $projPath = $csproj.FullName
        Write-Host "Creating EF migration 'InitialCreate'..."
        & dotnet ef migrations add InitialCreate --project $projPath --startup-project $projPath
        Write-Host "Applying database update..."
        & dotnet ef database update --project $projPath --startup-project $projPath
    }
}

Write-Host "Completed. Review created files under $Base."