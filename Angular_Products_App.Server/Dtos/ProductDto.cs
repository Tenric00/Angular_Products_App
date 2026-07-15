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