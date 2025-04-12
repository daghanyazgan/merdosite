using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using dagstore.Data;
using dagstore.Models;
using Microsoft.EntityFrameworkCore;
using dagstore.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace dagstore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, IFileService fileService, ILogger<AdminController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Settings()
        {
            return View();
        }

        public IActionResult CreateCategory()
        {
            return View(new CategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Kategori oluşturma validation hatası: {ErrorMessage}", error.ErrorMessage);
                }
                return View(model);
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now,
                ImageUrl = ""
            };

            try
            {
                if (model.Image != null)
                {
                    _logger.LogInformation("Kategori için görsel yükleniyor: {FileName}", model.Image.FileName);
                    category.ImageUrl = await _fileService.SaveFileAsync(model.Image, "categories");
                    _logger.LogInformation("Görsel kaydedildi: {ImageUrl}", category.ImageUrl);
                }
                else
                {
                    _logger.LogInformation("Kategori için görsel yüklenmedi, ImageUrl boş bırakılıyor.");
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Yeni kategori başarıyla veritabanına eklendi. ID: {CategoryId}", category.Id);

                TempData["SuccessMessage"] = "Kategori başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori oluşturulurken kritik bir hata oluştu. Kategori Adı: {CategoryName}", model.Name);
                ModelState.AddModelError("", "Kategori oluşturulurken beklenmedik bir hata oluştu. Lütfen tekrar deneyin veya sistem yöneticisine başvurun.");
            }
            
            return View(model);
        }

        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    product.ImageUrl = await _fileService.SaveFileAsync(image, "products");
                }

                product.CreatedAt = DateTime.Now;
                product.IsActive = true;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(product);
        }

        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile image)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null)
                    {
                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            _fileService.DeleteFile(product.ImageUrl);
                        }
                        // Yeni resmi kaydet
                        product.ImageUrl = await _fileService.SaveFileAsync(image, "products");
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Json(new { success = false });
            }

            try
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _fileService.DeleteFileAsync(product.ImageUrl);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ImageUrl = category.ImageUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.Categories.FindAsync(id);
                    if (category == null)
                    {
                        return NotFound();
                    }

                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.IsActive = model.IsActive;

                    if (model.Image != null)
                    {
                        // Eski resmi sil (varsa)
                        if (!string.IsNullOrEmpty(category.ImageUrl))
                        {
                            _fileService.DeleteFile(category.ImageUrl);
                        }
                        // Yeni resmi kaydet
                        category.ImageUrl = await _fileService.SaveFileAsync(model.Image, "categories");
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                    return RedirectToAction(nameof(Categories));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch(Exception ex)
                {
                     ModelState.AddModelError("", "Kategori güncellenirken bir hata oluştu: " + ex.Message);
                }
            }
            return View(model);
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                 TempData["ErrorMessage"] = "Kategori bulunamadı.";
                return RedirectToAction(nameof(Categories));
            }

            try
            {
                // İlişkili ürünler varsa silmeyi engelle (opsiyonel)
                var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
                if (hasProducts)
                {
                    TempData["ErrorMessage"] = "Bu kategoriye ait ürünler olduğu için silinemez.";
                    return RedirectToAction(nameof(Categories));
                }

                // Kategori resmini sil (varsa)
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                     _fileService.DeleteFile(category.ImageUrl); // Asenkron versiyonu varsa kullanın
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kategori silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Categories));
        }
    }
} 