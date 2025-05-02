using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Models.Product;

namespace App.Models.Product
{
    public class ProductModel
    {
        [Key]
        public int ProductId { set; get; }

        [Required(ErrorMessage = "Phải có tiêu đề sản phẩm")]
        [Display(Name = "Tiêu đề")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
        public string? Title { set; get; }

        [Display(Name = "Mô tả ngắn")]
        public string? Description { set; get; }

        [Display(Name = "Chuỗi định danh (url)", Prompt = "Nhập hoặc để trống tự phát sinh theo Title")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        public string? Slug { set; get; }

        [Display(Name = "Nội dung")]
        public string? Content { set; get; }

        [Display(Name = "Xuất bản")]
        public bool Published { set; get; }

        [Display(Name = "Người đăng")]
        public string? AuthorId { set; get; }

        [ForeignKey("AuthorId")]
        [Display(Name = "Người đăng")]
        public AppUser? Author { set; get; }

        [Display(Name = "Giá sản phẩm")]
        [Range(0, int.MaxValue, ErrorMessage = "Giá sản phẩm từ {1}")]
        [Required]
        public decimal Price { get; set; }

        [Display(Name = "Tỷ lệ giảm giá")]
        public decimal Discount { get; set; } = 0;

        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 0;

        [Display(Name = "Loại")]
        public string? Type { get; set; }

        [Display(Name = "Màu")]
        public string? Color { get; set; }

        [Display(Name = "Cân nặng")]
        public float Weight { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime DateCreated { set; get; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime DateUpdated { set; get; }
        public List<ProductCategoryProduct>? ProductCategoryProducts { get; set; }
        public List<ProductPhoto>? Photos { set; get; }
    }
}