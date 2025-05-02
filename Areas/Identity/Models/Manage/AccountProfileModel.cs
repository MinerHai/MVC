using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Areas.Identity.Models.ManageViewModels{
    public class AccountProfileModel
    {
        public string? FirstName { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? Gmail { get; set; }

    }
}