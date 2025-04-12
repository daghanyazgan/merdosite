using System.ComponentModel.DataAnnotations;

namespace dagstore.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        [Required]
        public string UserId { get; set; } = string.Empty;

        public User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
} 