using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using MVC.Models.Product;

namespace App.Models.Product
{

    [Table("ProductCategoryProduct")]
    public class ProductCategoryProduct
    {
        public int ProductId { set; get; }

        public int CategoryId { set; get; }

        [ForeignKey("ProductId")]
        public ProductModel? Product { set; get; }

        [ForeignKey("CategoryId")]
        public CategoryProduct? CategoryProducts { set; get; }
    }
}