using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Communication
{
    public class PatientRequestImage : ParentEntity
    {
        public string ImagePath { get; set; }
        public int PatientRequestId { get; set; }
    }
}
