using System.ComponentModel.DataAnnotations;

namespace dagstore.Models;

public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public List<ProductReview> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }

    [Required(ErrorMessage = "Puan seçiniz")]
    [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
    public int NewRating { get; set; }

    [Required(ErrorMessage = "Yorumunuzu yazın")]
    [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir")]
    public string NewComment { get; set; } = string.Empty;
} 