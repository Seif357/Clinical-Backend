using Domain.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Models
{
    public class Doctor : BaseUser
    {
        [Key]
        public int UserId { get; set; }
        public DateOnly? LicenseExpirationDate { get; set; }
        [Required]
        public required string ProfessionalPracticeLicense { get; set; }
        public bool? IsLicenseVerified {get; set; }
        public bool IsLicenseExpired => LicenseExpirationDate.HasValue && LicenseExpirationDate > DateOnly.FromDateTime(DateTime.Now);
        public required string IssuingAuthority { get; set; }
        [ForeignKey("UserId")]
        public AppUser DoctorData { get; set; }
    }
}
