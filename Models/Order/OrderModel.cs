using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Orders
{
    public class OrderModel
    {
        public int Id { get; set; }

        // Quan hệ với User (ApplicationUser - Identity)
        [Required]
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        // Thông tin người nhận tại thời điểm đặt hàng
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required, StringLength(200)]
        public string? StreetAddress { get; set; }

        [Required, StringLength(100)]
        public string? Country { get; set; }

        [Required, StringLength(100)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string? Phone { get; set; }

        // Thông tin đơn hàng
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, etc.

        [Required]
        [StringLength(50)]
        public string? PaymentMethod { get; set; } // COD, Online, etc.

        public bool IsPaid { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Danh sách sản phẩm trong đơn hàng
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}