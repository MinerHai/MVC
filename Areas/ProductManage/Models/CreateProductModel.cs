using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MVC.Migrations;
using MVC.Models.Product;

namespace App.Models.Product
{
    public class CreateProductModel : ProductModel
    {
        [Display(Name = "Loại sản phẩm ")]
        public int[] CategoryProductId { get; set; }
    }
}