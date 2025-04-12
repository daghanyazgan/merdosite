using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace dagstore.Models;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(50, ErrorMessage = "Kategori adı en fazla 50 karakter olabilir.")]
    [Display(Name = "Kategori Adı")]
    public string Name { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [Display(Name = "Kategori Görseli")]
    public IFormFile? Image { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
} 