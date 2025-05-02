using System.ComponentModel.DataAnnotations;

namespace App.Models.Blog
{
    public class CommentViewModel
    {
        [Required]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận không được để trống.")]
        [Display(Name = "Nội dung bình luận")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.")]
        public string Content { get; set; }
    }
}