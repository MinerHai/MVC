using System.ComponentModel.DataAnnotations;

namespace App.Models.Blog
{
    public class CreatePostModel : Post
    {
        [Display(Name = "Chuyên mục")]
        public int[] CategoryId { get; set; }
       
    }
}