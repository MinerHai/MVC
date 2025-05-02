using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models
{
    [Table("PostComment")]
    public class PostComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post? Post { get; set; }

        public string? AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public AppUser? Author { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận không được để trống")]
        [Display(Name = "Nội dung")]
        public string? Content { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

    }
}