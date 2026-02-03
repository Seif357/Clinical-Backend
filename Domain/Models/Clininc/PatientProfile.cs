using Domain.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Models.Clininc
{
    public class PatientProfile : AppUser
    {
        public int DoctorProfileId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string MedicalRecord { get; set; }
        public string PatientStatus { get; set; } = "Ongoing"; // Ongoing, Closed
        public DateTime? LastSyncAt { get; set; }
        public DoctorProfile Doctor { get; set; }
        public ICollection<DoctorRequest> DoctorRequests { get; set; }
        public ICollection<AppointmentRequest> AppointmentRequests { get; set; }

    }
}
