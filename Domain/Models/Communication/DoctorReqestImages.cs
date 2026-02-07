using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Communication
{
    public class DoctorReqestImages: ParentEntity
    {
        public string ImagePath { get; set; }
        public int DoctorRequestId { get; set; }
    }
}
