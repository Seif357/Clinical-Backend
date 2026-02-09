using Domain.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Models
{
    public class Patient
    {
        [Key]
        public int UserId { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public BloodType BloodType { get; set; }
        public MedicalRecord MedicalRecord { get; set; }
        public AppUser PatientData { get; set; }
    }
}
