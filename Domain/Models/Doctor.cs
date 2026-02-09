using Domain.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Models
{
    public class Doctor
    {
        [Key]
        public int UserId { get; set; }
        public DateOnly LicenseExpirationDate { get; set; }
        public string LicenseCertificate { get; set; }
        public AppUser DoctorData { get; set; }
    }
}
