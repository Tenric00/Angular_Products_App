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