using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Communication
{
    public class DoctorResponse : ParentEntity
    {
        public int PatientRequest_Id { get; set; }
        public int DoctorId { get; set; }
        public string Message { get; set; }
        public ICollection<DateTime?> AppointmentSchedule { get; set; }
    }
}
