using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace App.Models
{
    public class AppUser : IdentityUser
    {

        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? HomeAdress { get; set; }


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

        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? FirstName { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? LastName { get; set; }


        // [Required]       
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public string? AvatarImg { get; set; }
    }
}
