using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class Doctor : ParentEntity
    {
        public int user_id { get; set; }
        public DateOnly license_expiration_date { get; set; }
        public string license_certificate { get; set; }
    }
}
