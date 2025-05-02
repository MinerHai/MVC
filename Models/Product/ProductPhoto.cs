using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MVC.Models.Product;

namespace App.Models.Product
{
    [Table("ProductPhoto")]
    public class ProductPhoto
    {
        [Key]
        public int Id { set; get; }

        public string? FileName { set; get; }
        public int ProductId { set; get; }
        [ForeignKey("ProductId")]
        public ProductModel? Product { set; get; }
    }
}