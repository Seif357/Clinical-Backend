using Domain.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Models.Clininc
{
    public class PatientResponse
    {
        [Key]
        public int Id { get; set; }
        public int DoctorRequestId { get; set; }
        public int SubmittedByUserId { get; set; } // Can be doctor or patient
        public string ResponseText { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        // Navigation Properties
        public DoctorRequest DoctorRequest { get; set; }
        public AppUser SubmittedByUser { get; set; }
        public ICollection<PatientResponseImage> ResponseImages { get; set; }
    }
}
