using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Models.Product;
namespace App.Models.Orders
{
    public class OrderItem
    {
        public int Id { get; set; }

        // Quan hệ với Order
        public int OrderId { get; set; }
        public OrderModel? Order { get; set; }

        // Quan hệ với Product
        
        public int ProductId { get; set; }
        public ProductModel? Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }

}
