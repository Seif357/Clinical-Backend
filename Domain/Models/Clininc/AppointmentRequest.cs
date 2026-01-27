using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Models.Clininc
{
    public class AppointmentRequest
    {
        [Key]
        public int Id { get; set; }
        public int PatientProfileId { get; set; }
        public int DoctorProfileId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed, Cancelled
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PreferredDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string ReasonForVisit { get; set; }
        // Navigation Properties
        public PatientProfile PatientProfile { get; set; }
        public DoctorProfile DoctorProfile { get; set; }
    }
}
