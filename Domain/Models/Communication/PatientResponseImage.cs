using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Communication
{
    public class PatientResponseImage : ParentEntity
    {
        public string ImagePath { get; set; }
        public int PatientResponseId { get; set; }
    }
}
