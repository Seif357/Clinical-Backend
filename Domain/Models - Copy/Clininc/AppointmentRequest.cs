using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Models.Clininc
{
    public class Appointment_Request
    {
        public DateTime? PreferredDate { get; set; }
        public string ReasonForVisit { get; set; }
    }
}
