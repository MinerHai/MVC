using System.ComponentModel.DataAnnotations;
namespace App.Models.Blog
{
    public class UploadOneFile
    {
        [Required(ErrorMessage = "Phải chọn file upload")]
        [DataType(DataType.Upload)]
        [FileExtensions(Extensions = "png,jpg,jpeg,gif")]
        [Display(Name = "Chọn file upload")]
        public IFormFile? FileUpLoad { set; get; }
    }

}
