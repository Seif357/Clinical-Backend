using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Schedule
{
    public class ScheduleSlot: ParentEntity
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ScheduleId { get; set; }
        public int? patientId { get; set; }
        public bool IsBooked => patientId != null;
    }
}
