using dagstore.Models;
using System.Collections.Generic;

namespace dagstore.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> FeaturedProducts { get; set; }
        public IEnumerable<Product> AllProducts { get; set; }
    }
} 