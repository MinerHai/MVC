using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product
{
  [Table("CategoryProduct")]
  public class CategoryProduct
  {

    [Key]
    public int Id { get; set; }

    // Tiều đề Category
    [Required(ErrorMessage = "Phải có tên danh mục")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
    [Display(Name = "Tên danh mục")]
    public string? Title { get; set; }

    // Nội dung, thông tin chi tiết về Category
    [DataType(DataType.Text)]
    [Display(Name = "Nội dung danh mục")]
    public string? Content { set; get; }

    //chuỗi Url
    [Required(ErrorMessage = "Phải tạo url")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
    [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
    [Display(Name = "Url hiện thị")]
    public string? Slug { set; get; }

  }
}