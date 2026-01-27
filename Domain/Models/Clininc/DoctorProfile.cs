using Domain.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Models.Clininc
{
    public class DoctorProfile
    {
        [Key]
        [ForeignKey(nameof(User))]
        public int Id { get; set; }
        public AppUser User { get; set; }
        public ICollection<PatientProfile> Patients { get; set; }
        public ICollection<DoctorRequest> DoctorRequests { get; set; }
        public ICollection<AppointmentRequest> AppointmentRequests { get; set; }
    }
}
