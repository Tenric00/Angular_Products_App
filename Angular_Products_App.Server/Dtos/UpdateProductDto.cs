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