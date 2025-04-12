using System.ComponentModel.DataAnnotations;

namespace dagstore.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ürün açıklaması zorunludur")]
        [StringLength(500, ErrorMessage = "Ürün açıklaması en fazla 500 karakter olabilir")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, 1000000, ErrorMessage = "Fiyat 0.01 ile 1.000.000 arasında olmalıdır")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur")]
        [Range(0, 10000, ErrorMessage = "Stok miktarı 0 ile 10.000 arasında olmalıdır")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Kategori zorunludur")]
        public int CategoryId { get; set; }
    }
} 