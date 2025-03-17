using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models
{
    public class ContactModel
    {
        [Key]
        public int ID { set; get; }
        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(100)]

        public string? Name { set; get; }
        [Required]
        [StringLength(100)]
        public string Email { set; get; }
        public string? Desc { set; get; }
        public DateTime DateSent { set; get; }
        public string? Phone { set; get; }
    }
}
