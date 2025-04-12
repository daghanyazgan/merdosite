using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using dagstore.Models;
using dagstore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using dagstore.ViewModels;

namespace dagstore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var featuredProducts = await _context.Products
                .Where(p => p.IsActive && p.IsFeatured)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var allProducts = await _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                FeaturedProducts = featuredProducts,
                AllProducts = allProducts
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ana sayfa yüklenirken hata oluştu");
            return View(new HomeViewModel { 
                Categories = new List<Category>(), 
                FeaturedProducts = new List<Product>(),
                AllProducts = new List<Product>()
            });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    public async Task<IActionResult> CategoryProducts(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        ViewBag.CategoryName = category.Name;
        return View(category.Products.Where(p => p.IsActive).ToList());
    }

    public async Task<IActionResult> AllCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(categories);
    }
}
