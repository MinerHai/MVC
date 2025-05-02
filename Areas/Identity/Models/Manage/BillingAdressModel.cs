using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace App.Areas.Identity.Models.ManageViewModels
{
    public class BillingAdressModel
    {
        public string? FirstName { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
        public string? Gmail { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? StreetAdress { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? Country { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? City { get; set; }
        public decimal? ZipCode { get; set; }


    }
}