using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Models.Clininc
{
    public class DoctorRequest
    {
        [Key]
        public int Id { get; set; }
        public int DoctorProfileId { get; set; }
        public int PatientProfileId { get; set; }
        public string RequestType { get; set; }
        // Types: "AnalysisRequest", "ImageRequest", "ReportRequest", "Prescription", "GeneralRequest"
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Responded, Completed, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        // Navigation Properties
        public DoctorProfile DoctorProfile { get; set; }
        public PatientProfile PatientProfile { get; set; }
        public PatientResponse Responses { get; set; }
        public ICollection<DoctorRequestImage> RequestImages { get; set; }
    }
}
